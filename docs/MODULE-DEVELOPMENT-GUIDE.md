# 模块开发指南

> **文档版本**: 1.1  
> **更新日期**: 2026-02-14  
> **适用于**: Zss.BilliardHall MVP

本指南提供在 Zss.BilliardHall 中开发新业务模块的完整步骤和最佳实践。

---

## 📋 目录

1. [模块结构](#模块结构)
2. [创建新模块](#创建新模块)
3. [实现用例（Feature）](#实现用例feature)
4. [模块配置](#模块配置)
5. [测试](#测试)
6. [最佳实践](#最佳实践)
7. [常见问题](#常见问题)

---

## 模块结构

每个模块遵循以下标准结构：

```
src/Modules/ModuleName/
├── Features/                    # 用例集合
│   ├── CreateXxx/              # 创建用例
│   │   ├── CreateXxxCommand.cs
│   │   ├── CreateXxxCommandHandler.cs
│   │   ├── CreateXxxEndpoint.cs
│   │   └── CreateXxxResponse.cs
│   ├── GetXxxById/             # 查询用例
│   │   ├── GetXxxByIdQuery.cs
│   │   ├── GetXxxByIdQueryHandler.cs
│   │   ├── GetXxxByIdEndpoint.cs
│   │   └── XxxDto.cs
│   └── UpdateXxx/              # 更新用例
│       └── ...
├── Entities/                    # 领域实体（可选）
│   └── Xxx.cs
├── ModuleNameModule.cs         # 模块启动器（必须）
├── GlobalUsings.cs             # 全局引用（推荐）
├── ModuleName.csproj           # 项目文件
└── README.md                   # 模块文档（推荐）
```

---

## 创建新模块

### 步骤 1: 创建模块目录和项目文件

```bash
# 创建模块目录
mkdir -p src/Modules/YourModule

# 创建项目文件
cd src/Modules/YourModule
dotnet new classlib -n YourModule -f net10.0
```

### 步骤 2: 配置项目文件

编辑 `YourModule.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="WolverineFx.Http"/>
    <PackageReference Include="Marten"/>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Platform\Platform.csproj"/>
  </ItemGroup>
</Project>
```

### 步骤 3: 创建模块启动器

创建 `YourModuleModule.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule;

/// <summary>
/// YourModule 模块启动器
/// </summary>
public class YourModuleModule : IModule, IMartenModule
{
    public string Name => "YourModule";

    public void ConfigureServices(
        IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        // 注册模块特定的服务
        // 例如：validators、custom services 等
        // Wolverine 会自动发现 Handlers 和 Endpoints
    }

    public void ConfigureMarten(StoreOptions options)
    {
        // 配置 Marten 文档映射和索引
        options.Schema.For<YourEntity>()
            .UniqueIndex(x => x.SomeUniqueField);
    }
}
```
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        // 注册模块特定的服务
        // 例如：validators、custom services 等
        // Wolverine 会自动发现 Handlers 和 Endpoints
    }

    public void ConfigureMarten(StoreOptions options)
    {
        // 配置 Marten 文档映射和索引
        options.Schema.For<YourEntity>()
            .UniqueIndex(x => x.SomeUniqueField);
    }
}
```

### 步骤 4: 创建 GlobalUsings.cs（推荐）

```csharp
global using Marten;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Wolverine;
global using Wolverine.Http;
global using Zss.BilliardHall.Platform.Contracts;
```

### 步骤 5: 在 Host 中注册模块

编辑 `src/Host/Web/ModuleRegistry.cs` 和 `src/Host/Worker/ModuleRegistry.cs`：

```csharp
private static readonly IModule[] AllModules =
[
    new MemberModule(),
    new OrderModule(),
    new YourModuleModule(),  // 添加新模块
];
```

编辑 `src/Host/Web/Web.csproj` 和 `src/Host/Worker/Worker.csproj`：

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Modules\Members\Members.csproj"/>
  <ProjectReference Include="..\..\Modules\Orders\Orders.csproj"/>
  <ProjectReference Include="..\..\Modules\YourModule\YourModule.csproj"/>
</ItemGroup>
```

---

## 实现用例（Feature）

### 用例类型

1. **Command**：改变系统状态（创建、更新、删除）
2. **Query**：查询数据（不改变状态）

### 示例：创建实体用例

#### 1. 定义 Command

`Features/CreateYourEntity/CreateYourEntityCommand.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule.Features.CreateYourEntity;

/// <summary>
/// 创建 YourEntity 命令
/// </summary>
public sealed record CreateYourEntityCommand(
    string Name, 
    string Description);
```

#### 2. 定义 Response

`Features/CreateYourEntity/CreateYourEntityResponse.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule.Features.CreateYourEntity;

/// <summary>
/// 创建 YourEntity 响应
/// </summary>
public sealed record CreateYourEntityResponse(Guid Id);
```

#### 3. 实现 Handler

`Features/CreateYourEntity/CreateYourEntityCommandHandler.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule.Features.CreateYourEntity;

/// <summary>
/// 创建 YourEntity 命令处理器
/// </summary>
public class CreateYourEntityCommandHandler(IDocumentSession session)
{
    public Task<Guid> Handle(CreateYourEntityCommand command)
    {
        // 1. 验证已由 FluentValidation 在 Wolverine 管道中自动完成
        
        // 2. 创建实体
        var entity = new YourEntity
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // 3. 持久化
        session.Store(entity);

        // 4. 返回结果
        // 注意：Marten 事务会自动提交（IntegrateWithWolverine）
        return Task.FromResult(entity.Id);
    }
}
```

#### 4. 添加验证器（推荐）

`Features/CreateYourEntity/CreateYourEntityCommandValidator.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule.Features.CreateYourEntity;

/// <summary>
/// 创建 YourEntity 命令验证器
/// Wolverine 会自动在处理命令前调用此验证器
/// </summary>
public class CreateYourEntityCommandValidator : AbstractValidator<CreateYourEntityCommand>
{
    public CreateYourEntityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("名称不能为空")
            .MaximumLength(100).WithMessage("名称不能超过100个字符");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("描述不能超过500个字符");
    }
}
```

#### 5. 创建 Endpoint

`Features/CreateYourEntity/CreateYourEntityEndpoint.cs`：

```csharp
namespace Zss.BilliardHall.Modules.YourModule.Features.CreateYourEntity;

/// <summary>
/// 创建 YourEntity HTTP 端点
/// </summary>
public static class CreateYourEntityEndpoint
{
    /// <summary>
    /// POST /api/your-entities
    /// </summary>
    [WolverinePost("/api/your-entities")]
    public static Task<Guid> Create(
        CreateYourEntityCommand command, 
        IMessageBus bus, 
        CancellationToken ct = default) 
        => bus.InvokeAsync<Guid>(command, ct);
}
```

### 示例：查询用例

#### 1. 定义 Query

```csharp
public sealed record GetYourEntityByIdQuery(Guid Id);
```

#### 2. 定义 DTO

```csharp
public sealed record YourEntityDto(
    Guid Id, 
    string Name, 
    string Description, 
    DateTimeOffset CreatedAt);
```

#### 3. 实现 Handler

```csharp
public class GetYourEntityByIdQueryHandler(IDocumentSession session)
{
    public async Task<YourEntityDto?> Handle(
        GetYourEntityByIdQuery query, 
        CancellationToken ct)
    {
        var entity = await session.LoadAsync<YourEntity>(query.Id, ct);
        
        return entity == null 
            ? null 
            : new YourEntityDto(
                entity.Id, 
                entity.Name, 
                entity.Description, 
                entity.CreatedAt);
    }
}
```

#### 4. 创建 Endpoint

```csharp
public static class GetYourEntityByIdEndpoint
{
    [WolverineGet("/api/your-entities/{id}")]
    public static Task<YourEntityDto?> Get(
        Guid id, 
        IMessageBus bus, 
        CancellationToken ct = default) 
        => bus.InvokeAsync<YourEntityDto?>(new GetYourEntityByIdQuery(id), ct);
}
```

---

## 模块配置

### Marten 配置

在 `YourModuleModule.cs` 中配置 Marten：

```csharp
public void ConfigureMarten(StoreOptions options)
{
    // 配置文档映射
    options.Schema.For<YourEntity>()
        .UniqueIndex(x => x.SomeUniqueField)
        .Index(x => x.SomeIndexedField);

    // 配置事件存储（如果需要）
    options.Events.StreamIdentity = StreamIdentity.AsGuid;
}
```

### 服务注册

```csharp
public void ConfigureServices(
    IServiceCollection services, 
    IConfiguration configuration, 
    IHostEnvironment environment)
{
    // 注册验证器
    services.AddValidatorsFromAssemblyContaining<YourModuleModule>();

    // 注册自定义服务
    services.AddScoped<IYourService, YourService>();
}
```

---

## 测试

### 单元测试

创建 `src/tests/UnitTests/Modules/YourModule/` 目录：

```csharp
public class CreateYourEntityCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesEntity()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateYourEntityCommandHandler(session);
        var command = new CreateYourEntityCommand("Test", "Description");

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        session.Received(1).Store(Arg.Any<YourEntity>());
    }
}
```

### 集成测试

创建 `src/tests/IntegrationTests/Modules/YourModule/` 目录：

```csharp
public class YourModuleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public YourModuleIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateYourEntity_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var command = new CreateYourEntityCommand("Test", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/your-entities", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, result);
    }
}
```

---

## 最佳实践

### 1. 遵循垂直切片原则

✅ **推荐**：每个用例包含完整的端到端实现
```
Features/CreateMember/
├── CreateMemberCommand.cs
├── CreateMemberCommandHandler.cs
├── CreateMemberEndpoint.cs
└── CreateMemberResponse.cs
```

❌ **避免**：横向分层
```
Services/
  └── MemberService.cs
Repositories/
  └── MemberRepository.cs
Controllers/
  └── MemberController.cs
```

### 2. Handler 命名约定

- Command Handler: `{Command}Handler`
- Query Handler: `{Query}Handler`

### 3. Endpoint 命名约定

- Endpoint 类: `{Feature}Endpoint`
- 方法名: 使用简洁的动词（Create, Get, Update, Delete）

### 4. 实体设计

```csharp
public class YourEntity
{
    // 使用 Guid.CreateVersion7() 生成有序 UUID
    public Guid Id { get; set; }
    
    // 使用 required 关键字标记必需属性
    public required string Name { get; set; }
    
    // 使用 ? 标记可空属性
    public string? Description { get; set; }
    
    // 记录时间戳
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

### 5. 异常处理

使用 Platform 提供的异常体系：

```csharp
using Zss.BilliardHall.Platform.Exceptions;

// 业务异常
throw new DomainException("业务规则违反");

// 验证异常
throw new ValidationException("验证失败");

// 基础设施异常
throw new InfrastructureException("数据库连接失败");
```

### 6. 日志记录

Wolverine 会自动记录 Handler 执行日志，通常无需手动记录。如需自定义日志：

```csharp
public class YourHandler(ILogger<YourHandler> logger)
{
    public Task Handle(YourCommand command)
    {
        logger.LogInformation("处理命令: {CommandType}", command.GetType().Name);
        // ...
    }
}
```

---

## 常见问题

### Q: Handler 中需要手动调用 SaveChangesAsync 吗？

**A**: 不需要。使用 `IntegrateWithWolverine()` + `AutoApplyTransactions()` 后，Marten 事务会在消息处理管道中自动提交。

### Q: 如何在模块间通信？

**A**: 使用领域事件：
```csharp
// 发布事件
await bus.PublishAsync(new MemberCreatedEvent(memberId));

// 订阅事件
public class MemberCreatedEventHandler
{
    public Task Handle(MemberCreatedEvent evt)
    {
        // 处理事件
    }
}
```

### Q: 如何实现数据验证？

**A**: 使用 FluentValidation：
```csharp
public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

### Q: Endpoint 返回类型如何设置？

**A**: 
- 成功：返回具体类型（会自动序列化为 JSON）
- 失败：抛出异常（Wolverine 会自动转换为 HTTP 状态码）
- 自定义响应：返回 `IResult` 类型

---

## 参考资料

- [ADR-001: 模块化单体与垂直切片架构](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-002: Platform / Application / Host 三层启动体系](../adr/constitutional/ADR-002-platform-application-host-bootstrap.md)
- [Wolverine 文档](https://wolverine.netlify.app/)
- [Marten 文档](https://martendb.io/)
- [Members 模块示例](../../src/Modules/Members/README.md)

---

**本指南基于 MVP 最佳实践整理，持续更新中。**
