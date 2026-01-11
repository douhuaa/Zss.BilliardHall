# Wolverine 快速上手指南

> **目标读者**: 准备开始使用 Wolverine + 垂直切片架构开发的团队成员
>
> **阅读时间**: 15 分钟
>
> **前置要求**: 
> - 熟悉 C# 和 ASP.NET Core
> - 了解 CQRS 基本概念
> - 阅读过 [Wolverine模块化架构蓝图](./Wolverine模块化架构蓝图.md)

---

## 一、5 分钟上手

### 1.1 从零实现一个功能

假设要实现"创建台球桌"功能，完整步骤如下：

#### 第 1 步：创建切片目录

```bash
mkdir -p src/Modules/Tables/CreateTable
```

#### 第 2 步：定义 Command

`src/Modules/Tables/CreateTable/CreateTable.cs`:
```csharp
namespace Zss.BilliardHall.Modules.Tables.CreateTable;

/// <summary>
/// 创建台球桌命令
/// </summary>
public sealed record CreateTable(
    string Name,
    TableType Type,
    decimal HourlyRate
);
```

#### 第 3 步：实现 Handler

`src/Modules/Tables/CreateTable/CreateTableHandler.cs`:
```csharp
namespace Zss.BilliardHall.Modules.Tables.CreateTable;

public sealed class CreateTableHandler
{
    public async Task<Guid> Handle(
        CreateTable command,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        var table = new Table
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Type = command.Type,
            HourlyRate = command.HourlyRate,
            Status = TableStatus.Available,
            CreatedAt = DateTimeOffset.UtcNow
        };

        session.Store(table);
        await session.SaveChangesAsync(ct);

        return table.Id;
    }
}
```

#### 第 4 步：添加 HTTP 端点

`src/Modules/Tables/CreateTable/CreateTableEndpoint.cs`:
```csharp
namespace Zss.BilliardHall.Modules.Tables.CreateTable;

public sealed class CreateTableEndpoint
{
    [WolverinePost("/api/tables")]
    public static CreateTable Post(CreateTableRequest request)
        => new(request.Name, request.Type, request.HourlyRate);

    public sealed record CreateTableRequest(
        string Name,
        TableType Type,
        decimal HourlyRate
    );
}
```

#### 第 5 步：运行应用

```bash
dotnet run --project src/Bootstrapper/Bootstrapper.csproj
```

发送测试请求：
```bash
curl -X POST http://localhost:5000/api/tables \
  -H "Content-Type: application/json" \
  -d '{
    "name": "1号桌",
    "type": "Chinese",
    "hourlyRate": 50.00
  }'
```

**就这么简单！** 

Wolverine 会自动：
- 发现并注册 Handler
- 将 HTTP 请求路由到 Command
- 注入 `IDocumentSession`
- 执行 Handler
- 返回结果

---

## 二、核心概念速查

### 2.1 Wolverine 的 3 个核心约定

#### 约定 1：Handler 方法签名

```csharp
// ✅ 正确：第一个参数是 Command/Query/Event
public async Task<TResult> Handle(
    TCommand command,           // 第一个：消息
    IDependency1 dep1,          // 后续：自动注入的依赖
    IDependency2 dep2,
    CancellationToken ct = default)
{
    // ...
}
```

#### 约定 2：方法命名

```csharp
// ✅ 以下方法名都会被识别
public Task Handle(CreateOrder cmd) { }
public Task HandleAsync(CreateOrder cmd) { }

// ❌ 其他方法名不会被自动识别
public Task Process(CreateOrder cmd) { }  // 不会被识别
```

#### 约定 3：返回值类型

```csharp
// ✅ 所有这些返回类型都支持
public void Handle(Command cmd) { }
public Task Handle(Command cmd) { }
public TResult Handle(Command cmd) { }
public Task<TResult> Handle(Command cmd) { }

// 返回事件（自动发布）
public OrderCreated Handle(CreateOrder cmd)
{
    // ...
    return new OrderCreated(orderId);
}
```

### 2.2 命名规范速查表

| 类型 | 命名规则 | 示例 | 文件位置 |
|------|---------|------|----------|
| Command | 动词 + 名词 | `CreateTable`<br>`UpdateTable`<br>`DeleteTable` | `Modules/Tables/CreateTable/CreateTable.cs` |
| Query | Get/Find/List + 名词 | `GetTable`<br>`ListTables`<br>`FindAvailableTables` | `Modules/Tables/GetTable/GetTable.cs` |
| Event | 名词 + 动词过去式 | `TableCreated`<br>`TableReserved`<br>`TableReleased` | `Modules/Tables/Events/TableCreated.cs` |
| Handler | 消息名 + Handler | `CreateTableHandler`<br>`TableCreatedHandler` | `Modules/Tables/CreateTable/CreateTableHandler.cs` |
| Endpoint | 消息名 + Endpoint | `CreateTableEndpoint` | `Modules/Tables/CreateTable/CreateTableEndpoint.cs` |

### 2.3 项目结构模板

```text
Modules/
└── {ModuleName}/              # 如 Tables
    ├── {Feature}/             # 如 CreateTable
    │   ├── {Feature}.cs       # Command/Query 定义
    │   ├── {Feature}Handler.cs
    │   ├── {Feature}Endpoint.cs (可选)
    │   └── {Feature}Validator.cs (可选)
    ├── Events/
    │   └── {Event}.cs         # 如 TableCreated.cs
    ├── Domain/                # 模块内领域模型（可选）
    │   ├── {Entity}.cs        # 如 Table.cs
    │   └── {ValueObject}.cs
    └── {ModuleName}Module.cs  # 模块标记类（可选）
```

---

## 三、常见场景实现

### 3.1 场景 1：带验证的 Command

**需求**: 创建台球桌时，必须验证名称不为空且费率 > 0

**实现**:

1. 安装 FluentValidation:
```bash
dotnet add package FluentValidation
```

2. 创建验证器:
```csharp
// CreateTableValidator.cs
public sealed class CreateTableValidator : AbstractValidator<CreateTable>
{
    public CreateTableValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("桌台名称不能为空")
            .MaximumLength(50).WithMessage("桌台名称不能超过50个字符");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("小时费率必须大于0");
    }
}
```

3. 注册验证中间件（Program.cs）:
```csharp
builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
    
    // 启用自动验证
    opts.UseFluentValidation();
});
```

**就这样！** Wolverine 会在 Handler 执行前自动调用验证器。

### 3.2 场景 2：发布领域事件

**需求**: 台球桌创建后，发布 `TableCreated` 事件

**实现**:

1. 定义事件:
```csharp
// Events/TableCreated.cs
public sealed record TableCreated(
    Guid TableId,
    string TableName,
    DateTimeOffset CreatedAt
);
```

2. 修改 Handler 返回事件:
```csharp
public sealed class CreateTableHandler
{
    // 方式 1：返回事件（自动发布）
    public async Task<TableCreated> Handle(
        CreateTable command,
        IDocumentSession session)
    {
        var table = new Table { /* ... */ };
        session.Store(table);
        await session.SaveChangesAsync();

        // 返回事件，Wolverine 自动发布
        return new TableCreated(table.Id, table.Name, table.CreatedAt);
    }
}
```

或者手动发布：
```csharp
// 方式 2：手动发布
public async Task<Guid> Handle(
    CreateTable command,
    IDocumentSession session,
    IMessageBus bus)
{
    var table = new Table { /* ... */ };
    session.Store(table);
    await session.SaveChangesAsync();

    // 手动发布事件
    await bus.PublishAsync(new TableCreated(table.Id, table.Name, table.CreatedAt));

    return table.Id;
}
```

3. 创建事件处理器:
```csharp
// 在其他模块中监听事件
namespace Zss.BilliardHall.Modules.Devices.Handlers;

public sealed class TableCreatedHandler
{
    public async Task Handle(
        TableCreated @event,
        ILogger<TableCreatedHandler> logger)
    {
        logger.LogInformation(
            "台球桌 {TableName} 已创建，ID: {TableId}",
            @event.TableName,
            @event.TableId
        );

        // 可以触发其他操作，如初始化设备
    }
}
```

### 3.3 场景 3：跨模块命令调用

**需求**: Sessions 模块结束时段后，需要释放 Tables 模块的桌台

**实现**:

```csharp
// Sessions 模块
public sealed class EndSessionHandler
{
    public async Task Handle(
        EndSession command,
        IDocumentSession session,
        IMessageBus bus)
    {
        // 1. 结束会话
        var tableSession = await session.LoadAsync<TableSession>(command.SessionId);
        tableSession.End();
        session.Store(tableSession);
        await session.SaveChangesAsync();

        // 2. 发送命令到 Tables 模块（同步等待结果）
        await bus.InvokeAsync(new ReleaseTable(tableSession.TableId));
        
        // 3. 发布事件（异步，不等待）
        await bus.PublishAsync(new SessionEnded(command.SessionId, tableSession.TableId));
    }
}
```

**关键区别**:
- `InvokeAsync`: 同步调用，等待结果（跨模块 Command）
- `PublishAsync`: 异步发布，不等待（领域 Event）

### 3.4 场景 4：使用 Result 模式处理错误

**需求**: 预订桌台时，如果桌台不可用，返回友好错误

**实现**:

1. 定义 Result 类型（BuildingBlocks）:
```csharp
// BuildingBlocks/Contracts/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, string.Empty);
    public static Result<T> Fail(string error) => new(false, default!, error);
}
```

2. Handler 返回 Result:
```csharp
public sealed class ReserveTableHandler
{
    public async Task<Result<Guid>> Handle(
        ReserveTable command,
        IDocumentSession session)
    {
        var table = await session.LoadAsync<Table>(command.TableId);
        
        // 业务规则校验
        if (table == null)
            return Result<Guid>.Fail("台球桌不存在");
            
        if (table.Status != TableStatus.Available)
            return Result<Guid>.Fail($"台球桌不可用，当前状态：{table.Status}");

        // 执行业务逻辑
        var reservationId = Guid.NewGuid();
        table.Reserve(reservationId, command.MemberId);
        session.Store(table);
        await session.SaveChangesAsync();

        return Result<Guid>.Ok(reservationId);
    }
}
```

3. Endpoint 处理 Result:
```csharp
public sealed class ReserveTableEndpoint
{
    [WolverinePost("/api/tables/{tableId:guid}/reserve")]
    public static async Task<IResult> Post(
        Guid tableId,
        ReserveTableRequest request,
        IMessageBus bus)
    {
        var command = new ReserveTable(tableId, request.MemberId, request.Duration);
        var result = await bus.InvokeAsync<Result<Guid>>(command);

        return result.IsSuccess
            ? Results.Ok(new { reservationId = result.Value })
            : Results.BadRequest(new { error = result.Error });
    }
}
```

### 3.5 场景 5：添加事务支持

**需求**: 确保 Handler 操作原子性

**实现**:

```csharp
// 使用 [Transactional] 特性
public sealed class StartSessionHandler
{
    [Transactional]  // Wolverine 自动包装事务
    public async Task<Guid> Handle(
        StartSession command,
        IDocumentSession session,
        IMessageBus bus)
    {
        // 1. 创建会话
        var tableSession = new TableSession { /* ... */ };
        session.Store(tableSession);

        // 2. 更新桌台状态（跨聚合操作）
        var table = await session.LoadAsync<Table>(command.TableId);
        table.MarkAsOccupied(tableSession.Id);
        session.Store(table);

        // 3. 发布事件（通过 Outbox，保证一致性）
        await bus.PublishAsync(new SessionStarted(tableSession.Id));

        // Wolverine 自动：
        // - 提交 Marten 事务
        // - 持久化事件到 Outbox
        // - 异步发布事件
        
        return tableSession.Id;
    }
}
```

**或者全局启用事务**:
```csharp
// Program.cs
builder.Host.UseWolverine(opts =>
{
    // 所有 Handler 自动包装事务
    opts.Policies.AutoApplyTransactions();
});
```

---

## 四、调试技巧

### 4.1 查看 Wolverine 发现的 Handler

启动应用时，Wolverine 会输出日志：

```
Wolverine: Discovered handlers in assembly 'Zss.BilliardHall'
  - CreateTableHandler for CreateTable
  - UpdateTableHandler for UpdateTable
  - TableCreatedHandler for TableCreated
```

### 4.2 启用详细日志

在 `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Wolverine": "Debug",
      "Marten": "Debug"
    }
  }
}
```

### 4.3 查看消息状态（Outbox/Inbox）

```sql
-- 查看待发送的消息
SELECT * FROM wolverine_outgoing_messages;

-- 查看待处理的消息
SELECT * FROM wolverine_incoming_messages;

-- 查看失败的消息
SELECT * FROM wolverine_dead_letters;
```

### 4.4 测试 Handler（无需启动应用）

```csharp
[Fact]
public async Task Should_Create_Table_Successfully()
{
    // 使用 In-Memory Marten
    await using var store = DocumentStore.For(opts =>
    {
        opts.Connection(ConnectionSource.InMemoryConnectionString);
        opts.AutoCreateSchemaObjects = AutoCreate.All;
    });

    await using var session = store.LightweightSession();

    // 直接测试 Handler
    var handler = new CreateTableHandler();
    var command = new CreateTable("1号桌", TableType.Chinese, 50m);

    var tableId = await handler.Handle(command, session, CancellationToken.None);

    // 验证结果
    tableId.Should().NotBeEmpty();
    
    var table = await session.LoadAsync<Table>(tableId);
    table.Should().NotBeNull();
    table.Name.Should().Be("1号桌");
}
```

---

## 五、常见错误与解决

### 错误 1：Handler 未被发现

**现象**:
```
System.InvalidOperationException: Unknown message type 'CreateTable'
```

**原因**:
- Handler 方法名不是 `Handle` 或 `HandleAsync`
- Handler 类不是 `public`
- Handler 所在程序集未被扫描

**解决**:
```csharp
// ❌ 错误
internal class CreateTableHandler  // 不是 public
{
    public Task Process(CreateTable cmd) { }  // 方法名错误
}

// ✅ 正确
public sealed class CreateTableHandler
{
    public Task Handle(CreateTable cmd) { }
}
```

### 错误 2：循环依赖

**现象**:
```
System.InvalidOperationException: A circular dependency was detected
```

**原因**:
- 两个 Handler 互相通过 `IMessageBus.InvokeAsync` 调用

**解决**:
- 改用事件（`PublishAsync`）而非命令（`InvokeAsync`）
- 重新设计模块边界

### 错误 3：事务冲突

**现象**:
```
Marten.Exceptions.ConcurrencyException: Version mismatch
```

**原因**:
- 多个并发请求修改同一实体
- 未使用乐观并发控制

**解决**:
```csharp
// 启用乐观并发
public class Table
{
    public Guid Id { get; set; }
    public int Version { get; set; }  // Marten 自动管理版本号
}

// Marten 配置
opts.Schema.For<Table>().UseOptimisticConcurrency(true);
```

---

## 六、从 ABP 迁移速查

| ABP 概念 | Wolverine 等价物 | 说明 |
|----------|-----------------|------|
| `IRepository<T>` | `IDocumentSession` | 直接使用 Marten 会话 |
| `ApplicationService` | `Handler` | Handler 就是应用服务 |
| `AppService.Method()` | `IMessageBus.InvokeAsync()` | 跨模块调用 |
| `EventBus.Publish()` | `IMessageBus.PublishAsync()` | 发布事件 |
| `[UnitOfWork]` | `[Transactional]` | 事务管理 |
| `IObjectMapper` | ❌ 不需要 | 直接映射或使用记录类型 |
| `Dto` | `record` | 使用不可变记录类型 |

**关键差异**:
- ABP：分层架构，Repository 抽象
- Wolverine：垂直切片，直接使用 Marten

---

## 七、下一步学习

### 进阶主题

1. **Saga 工作流**: [Wolverine模块化架构蓝图 - 第五章](./Wolverine模块化架构蓝图.md#五saga跨步骤业务流程)
2. **外部消息传输**: RabbitMQ/Kafka 集成
3. **性能优化**: 批处理、异步队列
4. **监控诊断**: OpenTelemetry 集成

### 推荐资源

- [Wolverine 官方文档](https://wolverine.netlify.app/)
- [Marten 官方文档](https://martendb.io/)
- [本项目：系统模块划分](./系统模块划分.md)
- [本项目：Wolverine模块化架构蓝图](./Wolverine模块化架构蓝图.md)

---

## 八、速查清单

### 新建功能的完整步骤

- [ ] 确定功能属于哪个模块
- [ ] 创建功能切片目录 `Modules/{Module}/{Feature}/`
- [ ] 定义 Command/Query（`{Feature}.cs`）
- [ ] 实现 Handler（`{Feature}Handler.cs`）
- [ ] 添加 HTTP 端点（`{Feature}Endpoint.cs`）
- [ ] 添加验证器（可选，`{Feature}Validator.cs`）
- [ ] 定义领域事件（如需要，`Events/{Event}.cs`）
- [ ] 编写单元测试
- [ ] 提交代码

### 调试清单

- [ ] 检查 Handler 是否被 Wolverine 发现（启动日志）
- [ ] 启用 Debug 日志级别
- [ ] 检查数据库 Outbox/Inbox 表
- [ ] 使用 In-Memory 存储隔离测试
- [ ] 检查事务边界是否正确

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**状态**: ✅ 已审核
