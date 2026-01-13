# GitHub Copilot 常用指令模板

> **目的**: 为开发者提供快速生成符合项目架构规范的代码模板
>
> **适用范围**: Wolverine + 垂直切片架构 + Marten 持久化
>
> **版本**: 1.0.0

---

## 目录

1. [使用说明](#使用说明)
2. [完整功能切片模板](#完整功能切片模板)
3. [Command 相关模板](#command-相关模板)
4. [Query 相关模板](#query-相关模板)
5. [事件相关模板](#事件相关模板)
6. [Saga 工作流模板](#saga-工作流模板)
7. [领域模型模板](#领域模型模板)
8. [测试模板](#测试模板)
9. [快速参考](#快速参考)

---

## 使用说明

### 如何使用这些模板

1. **在代码编辑器中**，打开你想创建的文件
2. **在注释中输入模板指令**（复制本文档中的指令）
3. **GitHub Copilot 会自动生成代码**
4. **检查并调整生成的代码**，确保符合具体业务需求

### 注意事项

- ✅ 所有模板遵循垂直切片架构原则
- ✅ 所有模板符合项目代码风格规范
- ✅ 生成代码后请仔细检查业务逻辑
- ⚠️ 根据实际需求调整命名空间和类型
- ⚠️ 补充具体的业务规则和验证逻辑

---

## 完整功能切片模板

### 模板 1.1: 创建完整的 Command 功能切片

**使用场景**: 创建一个包含 Command、Handler、Endpoint、Validator 的完整写操作功能

**Copilot 指令**:
```
// 创建一个完整的垂直切片功能：创建订单
// 模块：Orders
// 功能：CreateOrder
// 包含：Command、Handler、Endpoint、Validator
// 使用 Wolverine + Marten + FluentValidation
// Handler 使用 [Transactional] 特性
// 返回 Result<Guid> 和级联事件 OrderCreated
// Endpoint 使用 WolverinePost，路径 /api/orders
```

**预期生成的文件结构**:
```
Modules/Orders/CreateOrder/
├── CreateOrder.cs              # Command 定义
├── CreateOrderHandler.cs       # Handler（业务逻辑）
├── CreateOrderEndpoint.cs      # HTTP 端点
└── CreateOrderValidator.cs     # 输入验证
```

**示例输出** (CreateOrder.cs):
```csharp
namespace Zss.BilliardHall.Modules.Orders.CreateOrder;

/// <summary>
/// 创建订单命令
/// Create order command
/// </summary>
public sealed record CreateOrder(
    Guid MemberId,
    Guid TableId,
    List<OrderItem> Items
);

public sealed record OrderItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
```

### 模板 1.2: 创建完整的 Query 功能切片

**使用场景**: 创建一个包含 Query、Handler、Endpoint 的完整查询功能

**Copilot 指令**:
```
// 创建一个完整的查询功能切片：获取订单详情
// 模块：Orders
// 功能：GetOrder
// 包含：Query、Handler、Endpoint
// 使用 Wolverine + Marten
// 返回 OrderDto
// Endpoint 使用 WolverineGet，路径 /api/orders/{orderId}
```

**预期生成的文件结构**:
```
Modules/Orders/GetOrder/
├── GetOrder.cs              # Query 定义
├── GetOrderHandler.cs       # Handler（查询逻辑）
└── GetOrderEndpoint.cs      # HTTP 端点
```

---

## Command 相关模板

### 模板 2.1: 创建 Command 定义

**Copilot 指令**:
```
// 创建一个 Command record 类型
// 功能：更新台球桌状态
// 命令名：UpdateTableStatus
// 参数：Guid tableId, TableStatus newStatus, string? reason
// 命名空间：Zss.BilliardHall.Modules.Tables.UpdateTableStatus
// 添加 XML 注释（中英文）
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.UpdateTableStatus;

/// <summary>
/// 更新台球桌状态命令
/// Update table status command
/// </summary>
public sealed record UpdateTableStatus(
    Guid TableId,
    TableStatus NewStatus,
    string? Reason = null
);
```

### 模板 2.2: 创建 Command Handler

**Copilot 指令**:
```
// 创建一个 Wolverine Handler
// 功能：处理 UpdateTableStatus 命令
// 使用 [Transactional] 特性
// 注入 IDocumentSession、ILogger
// 业务逻辑：
//   1. 加载 Table 聚合根
//   2. 验证状态转换是否合法
//   3. 更新状态
//   4. 持久化
//   5. 返回 Result<Unit> 和 TableStatusChanged 事件
// 包含详细的日志记录
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.UpdateTableStatus;

/// <summary>
/// 更新台球桌状态命令处理器
/// </summary>
public sealed class UpdateTableStatusHandler
{
    [Transactional]
    public async Task<(Result<Unit> Result, TableStatusChanged? Event)> Handle(
        UpdateTableStatus command,
        IDocumentSession session,
        ILogger<UpdateTableStatusHandler> logger,
        CancellationToken ct = default)
    {
        // 1. 加载聚合根
        var table = await session.LoadAsync<Table>(command.TableId, ct);
        if (table == null)
        {
            logger.LogWarning("台球桌不存在: {TableId}", command.TableId);
            return (Result.Fail<Unit>("台球桌不存在", "Tables:NotFound"), null);
        }

        // 2. 验证状态转换
        if (!table.CanTransitionTo(command.NewStatus))
        {
            logger.LogWarning(
                "非法的状态转换: {TableId} 从 {OldStatus} 到 {NewStatus}",
                command.TableId,
                table.Status,
                command.NewStatus
            );
            return (Result.Fail<Unit>("状态转换不合法", "Tables:InvalidStatusTransition"), null);
        }

        // 3. 执行业务操作
        var oldStatus = table.Status;
        table.UpdateStatus(command.NewStatus, command.Reason);

        // 4. 持久化（[Transactional] 会自动提交）
        session.Store(table);

        // 5. 返回结果和级联事件
        var @event = new TableStatusChanged(
            command.TableId,
            oldStatus,
            command.NewStatus,
            command.Reason,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "台球桌状态已更新: {TableId}, {OldStatus} -> {NewStatus}",
            command.TableId,
            oldStatus,
            command.NewStatus
        );

        return (Result.Success(Unit.Value), @event);
    }
}
```

### 模板 2.3: 创建 Command Endpoint

**Copilot 指令**:
```
// 创建一个 Wolverine HTTP Endpoint
// 功能：更新台球桌状态
// 使用 WolverinePut 特性
// 路径：/api/tables/{tableId}/status
// 参数：路由参数 tableId + 请求体 UpdateTableStatusRequest
// 映射到 UpdateTableStatus 命令
// 处理 Result 返回值，成功返回 200，失败返回 400
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.UpdateTableStatus;

/// <summary>
/// 更新台球桌状态端点
/// </summary>
public sealed class UpdateTableStatusEndpoint
{
    [WolverinePut("/api/tables/{tableId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Put(
        Guid tableId,
        UpdateTableStatusRequest request,
        IMessageBus bus)
    {
        var command = new UpdateTableStatus(
            tableId,
            request.NewStatus,
            request.Reason
        );

        var result = await bus.InvokeAsync<Result<Unit>>(command);

        return result.IsSuccess
            ? Results.Ok(new { message = "状态更新成功" })
            : Results.BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    public sealed record UpdateTableStatusRequest(
        TableStatus NewStatus,
        string? Reason = null
    );
}
```

### 模板 2.4: 创建 Command Validator

**Copilot 指令**:
```
// 创建一个 FluentValidation 验证器
// 功能：验证 UpdateTableStatus 命令
// 验证规则：
//   - TableId 不能为空
//   - NewStatus 必须是有效的枚举值
//   - Reason 如果提供，不能超过 200 字符
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.UpdateTableStatus;

/// <summary>
/// 更新台球桌状态命令验证器
/// </summary>
public sealed class UpdateTableStatusValidator : AbstractValidator<UpdateTableStatus>
{
    public UpdateTableStatusValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty()
            .WithMessage("台球桌ID不能为空");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("状态值无效");

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage("原因说明不能超过200个字符")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
```

---

## Query 相关模板

### 模板 3.1: 创建简单查询

**Copilot 指令**:
```
// 创建一个简单的查询功能
// 功能：根据ID获取台球桌信息
// Query：GetTable，参数 Guid tableId
// Handler：使用 Marten LoadAsync 查询
// 返回：TableDto（包含 Id, Name, Status, HourlyRate）
// Endpoint：GET /api/tables/{tableId}
```

### 模板 3.2: 创建列表查询

**Copilot 指令**:
```
// 创建一个列表查询功能
// 功能：查询可用台球桌列表
// Query：ListAvailableTables，参数：int? pageSize, int? pageNumber
// Handler：使用 Marten Query 查询，筛选 Status = Available
// 返回：PagedResult<TableDto>
// Endpoint：GET /api/tables/available
// 支持分页参数 ?pageSize=10&pageNumber=1
```

**预期输出** (ListAvailableTablesHandler.cs):
```csharp
namespace Zss.BilliardHall.Modules.Tables.ListAvailableTables;

public sealed class ListAvailableTablesHandler
{
    public async Task<PagedResult<TableDto>> Handle(
        ListAvailableTables query,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        var pageSize = query.PageSize ?? 10;
        var pageNumber = query.PageNumber ?? 1;

        // 查询总数
        var total = await session
            .Query<Table>()
            .Where(t => t.Status == TableStatus.Available)
            .CountAsync(ct);

        // 分页查询
        var tables = await session
            .Query<Table>()
            .Where(t => t.Status == TableStatus.Available)
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TableDto(
                t.Id,
                t.Name,
                t.Status,
                t.HourlyRate
            ))
            .ToListAsync(ct);

        return new PagedResult<TableDto>(
            tables,
            total,
            pageNumber,
            pageSize
        );
    }
}
```

### 模板 3.3: 创建复杂查询（多条件筛选）

**Copilot 指令**:
```
// 创建一个复杂查询功能
// 功能：搜索台球桌
// Query：SearchTables
// 参数：string? name, TableStatus? status, decimal? minRate, decimal? maxRate, int pageSize, int pageNumber
// Handler：使用 Marten Query，根据提供的参数动态构建查询条件
// 返回：PagedResult<TableDto>
// Endpoint：GET /api/tables/search
```

---

## 事件相关模板

### 模板 4.1: 创建领域事件

**Copilot 指令**:
```
// 创建一个领域事件 record
// 事件名：TableReserved
// 参数：Guid tableId, Guid memberId, DateTimeOffset reservedAt
// 命名空间：Zss.BilliardHall.Modules.Tables.Events
// 添加 XML 注释（中英文）
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.Events;

/// <summary>
/// 台球桌已预订事件
/// Table reserved event
/// </summary>
public sealed record TableReserved(
    Guid TableId,
    Guid MemberId,
    DateTimeOffset ReservedAt
);
```

### 模板 4.2: 创建事件处理器

**Copilot 指令**:
```
// 创建一个事件处理器
// 功能：监听 TableReserved 事件，创建打球会话
// Handler 名：TableReservedHandler
// 命名空间：Zss.BilliardHall.Modules.Sessions.Handlers
// 业务逻辑：
//   1. 创建 TableSession 实体
//   2. 持久化到 Marten
//   3. 记录日志
// 使用 [Transactional] 特性
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Sessions.Handlers;

/// <summary>
/// 台球桌预订事件处理器
/// </summary>
public sealed class TableReservedHandler
{
    [Transactional]
    public async Task Handle(
        TableReserved @event,
        IDocumentSession session,
        ILogger<TableReservedHandler> logger,
        CancellationToken ct = default)
    {
        // 创建打球会话
        var tableSession = new TableSession
        {
            Id = Guid.NewGuid(),
            TableId = @event.TableId,
            MemberId = @event.MemberId,
            StartTime = @event.ReservedAt,
            Status = SessionStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        session.Store(tableSession);

        logger.LogInformation(
            "已为台球桌 {TableId} 创建会话 {SessionId}",
            @event.TableId,
            tableSession.Id
        );
    }
}
```

### 模板 4.3: 创建集成事件（跨服务）

**Copilot 指令**:
```
// 创建一个集成事件（用于跨服务通信）
// 事件名：PaymentCompletedIntegrationEvent
// 参数：Guid paymentId, Guid orderId, decimal amount, DateTimeOffset completedAt
// 实现 IIntegrationEvent 接口
// 包含 EventId 和 OccurredAt 属性
// 命名空间：Zss.BilliardHall.BuildingBlocks.Contracts.IntegrationEvents
```

---

## Saga 工作流模板

### 模板 5.1: 创建 Saga 状态类

**Copilot 指令**:
```
// 创建一个 Wolverine Saga 状态类
// Saga 名：TableSessionSaga
// 状态字段：
//   - Guid SessionId（主键）
//   - Guid TableId
//   - Guid MemberId
//   - DateTimeOffset StartTime
//   - DateTimeOffset? EndTime
//   - SessionStatus Status
// 命名空间：Zss.BilliardHall.Modules.Sessions.Sagas
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Sessions.Sagas;

/// <summary>
/// 台球桌会话生命周期 Saga
/// </summary>
public sealed class TableSessionSaga : Saga
{
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public Guid MemberId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public SessionStatus Status { get; set; }
}
```

### 模板 5.2: 为 Saga 添加事件处理方法

**Copilot 指令**:
```
// 为 TableSessionSaga 添加事件处理方法
// 事件：SessionStarted、SessionPaused、SessionResumed、SessionEnded、PaymentCompleted
// 每个方法包含：
//   1. 状态验证（检查当前状态是否允许该操作）
//   2. 状态更新
//   3. PaymentCompleted 时调用 MarkCompleted() 结束 Saga
```

---

## 领域模型模板

### 模板 6.1: 创建聚合根

**Copilot 指令**:
```
// 创建一个聚合根实体
// 实体名：Table
// 属性：
//   - Guid Id
//   - string Name
//   - TableType Type
//   - TableStatus Status
//   - decimal HourlyRate
//   - DateTimeOffset CreatedAt
//   - DateTimeOffset? UpdatedAt
// 业务方法：
//   - Reserve(Guid reservationId, Guid memberId)
//   - Release()
//   - UpdateStatus(TableStatus newStatus, string? reason)
//   - CanTransitionTo(TableStatus newStatus) - 验证状态转换是否合法
// 命名空间：Zss.BilliardHall.Modules.Tables
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables;

/// <summary>
/// 台球桌聚合根
/// Table aggregate root
/// </summary>
public sealed class Table
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TableType Type { get; set; }
    public TableStatus Status { get; set; }
    public decimal HourlyRate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// 预订台球桌
    /// </summary>
    public void Reserve(Guid reservationId, Guid memberId)
    {
        if (Status != TableStatus.Available)
            throw new InvalidOperationException($"台球桌状态为 {Status}，无法预订");

        Status = TableStatus.Reserved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 释放台球桌
    /// </summary>
    public void Release()
    {
        if (Status == TableStatus.Maintenance)
            throw new InvalidOperationException("维护中的台球桌无法直接释放");

        Status = TableStatus.Available;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    public void UpdateStatus(TableStatus newStatus, string? reason)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException($"无法从 {Status} 转换到 {newStatus}");

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 检查是否可以转换到目标状态
    /// </summary>
    public bool CanTransitionTo(TableStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (TableStatus.Available, TableStatus.Reserved) => true,
            (TableStatus.Available, TableStatus.Maintenance) => true,
            (TableStatus.Reserved, TableStatus.Available) => true,
            (TableStatus.Reserved, TableStatus.Occupied) => true,
            (TableStatus.Occupied, TableStatus.Available) => true,
            (TableStatus.Maintenance, TableStatus.Available) => true,
            _ => Status == newStatus // 允许设置为相同状态
        };
    }
}
```

### 模板 6.2: 创建值对象

**Copilot 指令**:
```
// 创建一个值对象 record
// 值对象名：Money
// 属性：decimal Amount, string Currency
// 业务方法：
//   - Add(Money other) - 相加（必须同币种）
//   - Multiply(decimal factor) - 乘法
// 包含验证逻辑：Amount >= 0，Currency 不为空
// 命名空间：Zss.BilliardHall.BuildingBlocks.Domain
```

### 模板 6.3: 创建枚举和错误码

**Copilot 指令**:
```
// 创建一个模块的错误码常量类
// 模块：Tables
// 类名：TableErrorCodes
// 错误码格式：Tables:{ErrorKey}
// 包含常见错误：NotFound, Unavailable, InvalidStatus, AlreadyReserved
// 命名空间：Zss.BilliardHall.Modules.Tables
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables;

/// <summary>
/// 台球桌模块错误码
/// </summary>
public static class TableErrorCodes
{
    public const string NotFound = "Tables:NotFound";
    public const string Unavailable = "Tables:Unavailable";
    public const string InvalidStatus = "Tables:InvalidStatus";
    public const string AlreadyReserved = "Tables:AlreadyReserved";
    public const string InvalidStatusTransition = "Tables:InvalidStatusTransition";
}
```

---

## 测试模板

### 模板 7.1: 创建 Handler 单元测试

**Copilot 指令**:
```
// 创建一个 xUnit 测试类
// 测试目标：CreateTableHandler
// 使用 In-Memory Marten 存储
// 测试用例：
//   1. Should_Create_Table_Successfully - 成功创建台球桌
//   2. Should_Fail_When_Name_Already_Exists - 名称重复时失败
// 使用 FluentAssertions 进行断言
```

**预期输出**:
```csharp
namespace Zss.BilliardHall.Modules.Tables.Tests;

public sealed class CreateTableHandlerTests : IAsyncLifetime
{
    private IDocumentStore _store = null!;

    public async Task InitializeAsync()
    {
        _store = DocumentStore.For(opts =>
        {
            opts.Connection(ConnectionSource.InMemoryConnectionString);
            opts.AutoCreateSchemaObjects = AutoCreate.All;
        });

        await _store.Advanced.Clean.CompletelyRemoveAllAsync();
    }

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
    }

    [Fact]
    public async Task Should_Create_Table_Successfully()
    {
        // Arrange
        await using var session = _store.LightweightSession();
        var handler = new CreateTableHandler();
        var command = new CreateTable(
            "1号桌",
            TableType.Chinese,
            50m
        );

        // Act
        var result = await handler.Handle(
            command,
            session,
            Mock.Of<ILogger<CreateTableHandler>>(),
            CancellationToken.None
        );

        // Assert
        result.Result.IsSuccess.Should().BeTrue();
        result.Result.Value.Should().NotBeEmpty();

        var table = await session.LoadAsync<Table>(result.Result.Value);
        table.Should().NotBeNull();
        table!.Name.Should().Be("1号桌");
        table.Type.Should().Be(TableType.Chinese);
        table.HourlyRate.Should().Be(50m);
    }

    [Fact]
    public async Task Should_Fail_When_Name_Already_Exists()
    {
        // Arrange
        await using var session = _store.LightweightSession();
        
        // 预先创建一个台球桌
        var existingTable = new Table
        {
            Id = Guid.NewGuid(),
            Name = "1号桌",
            Type = TableType.Chinese,
            HourlyRate = 50m,
            Status = TableStatus.Available,
            CreatedAt = DateTimeOffset.UtcNow
        };
        session.Store(existingTable);
        await session.SaveChangesAsync();

        var handler = new CreateTableHandler();
        var command = new CreateTable("1号桌", TableType.Chinese, 60m);

        // Act
        var result = await handler.Handle(
            command,
            session,
            Mock.Of<ILogger<CreateTableHandler>>(),
            CancellationToken.None
        );

        // Assert
        result.Result.IsSuccess.Should().BeFalse();
        result.Result.Error.Should().Contain("已存在");
    }
}
```

### 模板 7.2: 创建集成测试

**Copilot 指令**:
```
// 创建一个集成测试类
// 测试目标：完整的台球桌预订流程
// 使用 WebApplicationFactory
// 测试步骤：
//   1. 创建台球桌（POST /api/tables）
//   2. 预订台球桌（POST /api/tables/{id}/reserve）
//   3. 验证状态已更新（GET /api/tables/{id}）
// 使用真实的 Wolverine 和 Marten 配置
```

---

## 快速参考

### 命名规范速查

| 类型 | 命名规则 | 示例 |
|------|---------|------|
| Command | 动词 + 名词 | `CreateTable`, `UpdateMember`, `ProcessPayment` |
| Query | Get/Find/List + 名词 | `GetTable`, `ListMembers`, `FindAvailableTables` |
| Event | 名词 + 过去式动词 | `TableCreated`, `MemberRegistered`, `PaymentCompleted` |
| Handler | 消息名 + Handler | `CreateTableHandler`, `TableCreatedHandler` |
| Endpoint | 消息名 + Endpoint | `CreateTableEndpoint`, `GetTableEndpoint` |
| Validator | 消息名 + Validator | `CreateTableValidator` |

### 文件组织速查

```
Modules/{ModuleName}/
├── {Feature}/                    # 功能切片
│   ├── {Feature}.cs              # Command/Query
│   ├── {Feature}Handler.cs       # Handler
│   ├── {Feature}Endpoint.cs      # HTTP 端点（可选）
│   └── {Feature}Validator.cs     # 验证器（可选）
├── Events/                       # 模块事件
│   ├── {Event}.cs
│   └── ...
├── Handlers/                     # 事件处理器（可选）
│   └── {EventHandler}.cs
├── Sagas/                        # Saga 工作流（可选）
│   └── {Saga}.cs
├── {Entity}.cs                   # 聚合根/实体
├── {ValueObject}.cs              # 值对象
├── {ErrorCodes}.cs               # 错误码
└── {ModuleName}Module.cs         # 模块标记（可选）
```

### Wolverine 特性速查

| 特性 | 用途 | 示例 |
|------|------|------|
| `[Transactional]` | 自动事务管理 | 加在 Handler 方法上 |
| `[WolverinePost]` | HTTP POST 端点 | `[WolverinePost("/api/tables")]` |
| `[WolverineGet]` | HTTP GET 端点 | `[WolverineGet("/api/tables/{id}")]` |
| `[WolverinePut]` | HTTP PUT 端点 | `[WolverinePut("/api/tables/{id}")]` |
| `[WolverineDelete]` | HTTP DELETE 端点 | `[WolverineDelete("/api/tables/{id}")]` |

### Result 模式速查

```csharp
// 成功结果
return (Result.Success(value), @event);

// 失败结果（带错误码）
return (Result.Fail<T>("错误消息", "Module:ErrorKey"), null);

// 不返回事件
return (Result.Success(value), null);
```

### 常用依赖注入速查

```csharp
public async Task Handle(
    TCommand command,              // 第一个参数：消息
    IDocumentSession session,      // Marten 会话
    IMessageBus bus,               // 消息总线
    ILogger<THandler> logger,      // 日志记录器
    CancellationToken ct = default // 取消令牌
)
```

---

## 附录：完整示例

### 示例：完整的会员充值功能

**目录结构**:
```
Modules/Members/TopUpBalance/
├── TopUpBalance.cs
├── TopUpBalanceHandler.cs
├── TopUpBalanceEndpoint.cs
└── TopUpBalanceValidator.cs
```

**TopUpBalance.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

/// <summary>
/// 会员充值命令
/// Top up member balance command
/// </summary>
public sealed record TopUpBalance(
    Guid MemberId,
    decimal Amount,
    string? Remark = null
);
```

**TopUpBalanceHandler.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

public sealed class TopUpBalanceHandler
{
    [Transactional]
    public async Task<(Result<Unit> Result, BalanceToppedUp? Event)> Handle(
        TopUpBalance command,
        IDocumentSession session,
        ILogger<TopUpBalanceHandler> logger,
        CancellationToken ct = default)
    {
        // 1. 加载会员
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member == null)
        {
            logger.LogWarning("会员不存在: {MemberId}", command.MemberId);
            return (Result.Fail<Unit>("会员不存在", MemberErrorCodes.NotFound), null);
        }

        // 2. 执行充值
        member.TopUp(command.Amount);
        session.Store(member);

        // 3. 返回结果和事件
        var @event = new BalanceToppedUp(
            member.Id,
            command.Amount,
            member.Balance,
            command.Remark,
            DateTimeOffset.UtcNow
        );

        logger.LogInformation(
            "会员充值成功: {MemberId}, 金额: {Amount:F2}, 余额: {Balance:F2}",
            member.Id,
            command.Amount,
            member.Balance
        );

        return (Result.Success(Unit.Value), @event);
    }
}
```

**TopUpBalanceEndpoint.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

public sealed class TopUpBalanceEndpoint
{
    [WolverinePost("/api/members/{memberId:guid}/top-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Post(
        Guid memberId,
        TopUpBalanceRequest request,
        IMessageBus bus)
    {
        var command = new TopUpBalance(
            memberId,
            request.Amount,
            request.Remark
        );

        var result = await bus.InvokeAsync<Result<Unit>>(command);

        return result.IsSuccess
            ? Results.Ok(new { message = "充值成功" })
            : Results.BadRequest(new { error = result.Error, code = result.ErrorCode });
    }

    public sealed record TopUpBalanceRequest(
        decimal Amount,
        string? Remark = null
    );
}
```

**TopUpBalanceValidator.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.TopUpBalance;

public sealed class TopUpBalanceValidator : AbstractValidator<TopUpBalance>
{
    public TopUpBalanceValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("会员ID不能为空");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("充值金额必须大于0")
            .LessThanOrEqualTo(10000)
            .WithMessage("单次充值不能超过10000元");

        RuleFor(x => x.Remark)
            .MaximumLength(200)
            .WithMessage("备注不能超过200个字符")
            .When(x => !string.IsNullOrEmpty(x.Remark));
    }
}
```

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0.0 | 2026-01-13 | 初始版本，包含完整的 Command、Query、Event、Saga、领域模型和测试模板 |

---

**最后更新**: 2026-01-13  
**负责人**: 开发团队  
**状态**: ✅ 已审核
