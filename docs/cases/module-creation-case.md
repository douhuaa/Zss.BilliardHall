# 创建新模块案例

**难度**：🟡 中等  
**相关 ADR**：[ADR-0001](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md), [ADR-0003](../adr/constitutional/ADR-0003-namespace-rules.md)  
**作者**：@copilot  
**日期**：2026-01-29  
**标签**：模块化, 垂直切片, 领域驱动设计

---

## 适用场景

当你需要为新的业务能力创建一个新模块时，本案例展示完整的模块创建流程。

**适用于**：
- 系统需要支持新的业务能力
- 现有模块职责过重需要拆分
- 需要遵循模块化单体架构创建新模块

---

## 背景

根据 ADR-0001，系统采用模块化单体架构，每个模块代表一个业务能力。模块之间必须强隔离，仅通过事件、契约或原始类型通信。

### 为什么要创建新模块

- **业务能力隔离**：每个业务能力独立模块
- **团队独立开发**：不同团队负责不同模块
- **技术栈灵活**：模块内可使用不同技术选择
- **未来可拆分**：为微服务转型做准备

---

## 解决方案

### 架构设计

假设我们要创建一个"会员管理"(Members)模块，负责会员注册、等级管理等功能。

#### 模块结构

```
src/Modules/Members/
├── Domain/                    # 领域模型
│   ├── Entities/
│   │   └── Member.cs
│   ├── ValueObjects/
│   │   └── MemberLevel.cs
│   └── Events/
│       └── MemberRegistered.cs
├── Contracts/                 # 对外契约（DTO）
│   ├── Queries/
│   │   └── GetMemberById.cs
│   └── Events/
│       └── MemberRegisteredEvent.cs
├── UseCases/                  # 用例（垂直切片）
│   ├── RegisterMember/
│   │   ├── RegisterMember.cs          # Command
│   │   ├── RegisterMemberHandler.cs   # Handler
│   │   └── RegisterMemberValidator.cs # 验证器
│   └── GetMemberDetails/
│       ├── GetMemberDetails.cs        # Query
│       └── GetMemberDetailsHandler.cs # Handler
├── Infrastructure/            # 基础设施
│   ├── Persistence/
│   │   └── MemberRepository.cs
│   └── DependencyInjection.cs
└── Members.csproj
```

### 代码实现

#### 步骤 1：创建项目文件

```xml
<!-- src/Modules/Members/Members.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>Zss.BilliardHall.Modules.Members</RootNamespace>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Platform 依赖 -->
    <ProjectReference Include="../../Platform/Platform.csproj" />
    <!-- BuildingBlocks 依赖 -->
    <ProjectReference Include="../../BuildingBlocks/BuildingBlocks.csproj" />
  </ItemGroup>

  <!-- 中央包管理：版本号在 Directory.Packages.props 中定义 -->
  <ItemGroup>
    <PackageReference Include="MediatR" />
    <PackageReference Include="FluentValidation" />
  </ItemGroup>
</Project>
```

#### 步骤 2：创建领域模型

```csharp
// Domain/Entities/Member.cs
namespace Zss.BilliardHall.Modules.Members.Domain.Entities;

public class Member : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public MemberLevel Level { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    // 私有构造函数：强制通过工厂方法创建
    private Member() { }

    // 工厂方法
    public static Member Register(string name, string email)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Level = MemberLevel.Regular,
            RegisteredAt = DateTime.UtcNow
        };

        // 发布领域事件
        member.AddDomainEvent(new MemberRegistered(member.Id, name, email));

        return member;
    }

    // 业务方法
    public void UpgradeLevel(MemberLevel newLevel)
    {
        if (newLevel <= Level)
            throw new InvalidOperationException("新等级必须高于当前等级");

        var oldLevel = Level;
        Level = newLevel;

        AddDomainEvent(new MemberLevelUpgraded(Id, oldLevel, newLevel));
    }
}

// Domain/ValueObjects/MemberLevel.cs
namespace Zss.BilliardHall.Modules.Members.Domain.ValueObjects;

public enum MemberLevel
{
    Regular = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}
```

#### 步骤 3：创建领域事件

```csharp
// Domain/Events/MemberRegistered.cs
namespace Zss.BilliardHall.Modules.Members.Domain.Events;

/// <summary>
/// 领域事件：会员注册（模块内部使用）
/// </summary>
public record MemberRegistered(
    Guid MemberId,
    string Name,
    string Email
) : IDomainEvent;
```

#### 步骤 4：创建契约（对外暴露）

```csharp
// Contracts/Events/MemberRegisteredEvent.cs
namespace Zss.BilliardHall.Modules.Members.Contracts.Events;

/// <summary>
/// 集成事件：会员注册（跨模块通信）
/// </summary>
public record MemberRegisteredEvent(
    Guid MemberId,
    string MemberName,
    DateTime RegisteredAt
) : IIntegrationEvent;

// Contracts/Queries/GetMemberById.cs
namespace Zss.BilliardHall.Modules.Members.Contracts.Queries;

/// <summary>
/// 查询契约：根据 ID 获取会员（跨模块查询）
/// </summary>
public record MemberDto(
    Guid Id,
    string Name,
    string Email,
    string Level
);
```

#### 步骤 5：创建用例（垂直切片）

```csharp
// UseCases/RegisterMember/RegisterMember.cs
namespace Zss.BilliardHall.Modules.Members.UseCases.RegisterMember;

/// <summary>
/// Command：注册会员
/// </summary>
public record RegisterMember(
    string Name,
    string Email
) : IRequest<Guid>;  // Command 仅返回 ID

// UseCases/RegisterMember/RegisterMemberHandler.cs
namespace Zss.BilliardHall.Modules.Members.UseCases.RegisterMember;

public class RegisterMemberHandler : IRequestHandler<RegisterMember, Guid>
{
    private readonly IMemberRepository _repository;
    private readonly IEventBus _eventBus;

    public RegisterMemberHandler(
        IMemberRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(
        RegisterMember command,
        CancellationToken cancellationToken)
    {
        // 1. 创建领域模型
        var member = Member.Register(command.Name, command.Email);

        // 2. 保存到仓储
        await _repository.SaveAsync(member, cancellationToken);

        // 3. 发布集成事件（跨模块通信）
        var integrationEvent = new MemberRegisteredEvent(
            member.Id,
            member.Name,
            member.RegisteredAt
        );
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        // 4. 仅返回 ID
        return member.Id;
    }
}

// UseCases/RegisterMember/RegisterMemberValidator.cs
namespace Zss.BilliardHall.Modules.Members.UseCases.RegisterMember;

public class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

#### 步骤 6：创建 Query Handler

```csharp
// UseCases/GetMemberDetails/GetMemberDetails.cs
namespace Zss.BilliardHall.Modules.Members.UseCases.GetMemberDetails;

public record GetMemberDetails(Guid MemberId) : IRequest<MemberDto>;

// UseCases/GetMemberDetails/GetMemberDetailsHandler.cs
namespace Zss.BilliardHall.Modules.Members.UseCases.GetMemberDetails;

public class GetMemberDetailsHandler : IRequestHandler<GetMemberDetails, MemberDto>
{
    private readonly IMemberRepository _repository;

    public GetMemberDetailsHandler(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<MemberDto> Handle(
        GetMemberDetails query,
        CancellationToken cancellationToken)
    {
        var member = await _repository.GetByIdAsync(query.MemberId, cancellationToken);
        
        if (member == null)
            throw new NotFoundException($"Member {query.MemberId} not found");

        // Query 返回契约 DTO
        return new MemberDto(
            member.Id,
            member.Name,
            member.Email,
            member.Level.ToString()
        );
    }
}
```

#### 步骤 7：创建仓储

```csharp
// Infrastructure/Persistence/IMemberRepository.cs
namespace Zss.BilliardHall.Modules.Members.Infrastructure.Persistence;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(Member member, CancellationToken cancellationToken = default);
}

// Infrastructure/Persistence/MemberRepository.cs
public class MemberRepository : IMemberRepository
{
    private readonly IDocumentSession _session;

    public MemberRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _session.LoadAsync<Member>(id, cancellationToken);
    }

    public async Task SaveAsync(Member member, CancellationToken cancellationToken = default)
    {
        _session.Store(member);
        await _session.SaveChangesAsync(cancellationToken);
    }
}
```

#### 步骤 8：依赖注入配置

```csharp
// Infrastructure/DependencyInjection.cs
namespace Zss.BilliardHall.Modules.Members.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMembersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 注册 MediatR Handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // 注册 FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // 注册仓储
        services.AddScoped<IMemberRepository, MemberRepository>();

        return services;
    }
}
```

#### 步骤 9：在 Application 层注册模块

```csharp
// src/Application/DependencyInjection.cs
public static IServiceCollection AddApplication(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... 其他模块

    // 注册 Members 模块
    services.AddMembersModule(configuration);

    return services;
}
```

### 测试验证

#### 单元测试

```csharp
// tests/Modules.Members.Tests/UseCases/RegisterMemberHandlerTests.cs
namespace Modules.Members.Tests.UseCases;

public class RegisterMemberHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesMember()
    {
        // Arrange
        var repository = Substitute.For<IMemberRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var handler = new RegisterMemberHandler(repository, eventBus);
        
        var command = new RegisterMember("John Doe", "john@example.com");

        // Act
        var memberId = await handler.Handle(command, CancellationToken.None);

        // Assert
        memberId.Should().NotBeEmpty();
        await repository.Received(1).SaveAsync(
            Arg.Is<Member>(m => m.Name == "John Doe"),
            Arg.Any<CancellationToken>()
        );
        await eventBus.Received(1).PublishAsync(
            Arg.Any<MemberRegisteredEvent>(),
            Arg.Any<CancellationToken>()
        );
    }
}
```

#### 架构测试

```csharp
// tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs
[Fact]
public void Members_Module_Should_Not_Reference_Orders_Module()
{
    var result = Types.InAssembly(typeof(Program).Assembly)
        .That()
        .ResideInNamespace("Zss.BilliardHall.Modules.Members")
        .ShouldNot()
        .HaveDependencyOn("Zss.BilliardHall.Modules.Orders")
        .GetResult();

    Assert.True(result.IsSuccessful);
}
```

---

## 常见陷阱

### 1. 模块间直接引用

❌ **错误**：
```csharp
// Members 模块直接使用 Orders 的领域模型
using Zss.BilliardHall.Modules.Orders.Domain;

public class Member
{
    public List<Order> Orders { get; set; }  // ❌ 跨模块依赖
}
```

✅ **正确**：
```csharp
// 通过契约查询或事件通信
public class MemberOrdersQuery : IRequest<List<OrderDto>>
{
    public Guid MemberId { get; set; }
}
```

### 2. 在 Handler 中包含业务逻辑

❌ **错误**：
```csharp
public class RegisterMemberHandler
{
    public async Task<Guid> Handle(RegisterMember command, ...)
    {
        var member = new Member();
        member.Id = Guid.NewGuid();
        member.Name = command.Name;
        
        // ❌ 业务逻辑在 Handler 中
        if (command.Email.Contains("@vip.com"))
        {
            member.Level = MemberLevel.Gold;
        }
        
        await _repository.SaveAsync(member);
        return member.Id;
    }
}
```

✅ **正确**：
```csharp
// 业务逻辑在领域模型中
public class Member
{
    public static Member Register(string name, string email)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email
        };
        
        // 业务逻辑在领域模型中
        member.DetermineInitialLevel(email);
        
        return member;
    }
    
    private void DetermineInitialLevel(string email)
    {
        Level = email.Contains("@vip.com") 
            ? MemberLevel.Gold 
            : MemberLevel.Regular;
    }
}
```

### 3. Command 返回业务数据

❌ **错误**：
```csharp
public record RegisterMember(...) : IRequest<MemberDto>;  // ❌ 返回 DTO
```

✅ **正确**：
```csharp
public record RegisterMember(...) : IRequest<Guid>;  // ✅ 仅返回 ID
```

---

## 参考资料

- [ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0003：命名空间与项目结构规范](../adr/constitutional/ADR-0003-namespace-rules.md)
- [ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [架构设计指南](../guides/architecture-design-guide.md)
- [跨模块通信指南](../guides/cross-module-communication.md)

---

**维护**：Tech Lead  
**状态**：✅ Active  
**版本**：1.0
