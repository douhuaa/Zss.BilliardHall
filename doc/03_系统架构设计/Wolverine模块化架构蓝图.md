# Wolverine 模块化架构蓝图

> **定位**: 可直接落地、非 PPT 架构的实战型 Wolverine 模块化规划
> 
> **适用场景**: 自助台球系统等业务能力清晰的中小型应用
>
> **核心观点**: Wolverine ≠ MediatR 替代品。它是 HTTP + Command Bus + Message Bus + Workflow 引擎的融合体

---

## 一、总体架构立场

### 1.1 核心原则

#### 原则 1: 100% 垂直切片（Vertical Slice）

**禁止传统分层**:
- ❌ 不要 `Application` / `Domain` / `Infrastructure` 分层
- ❌ 分层只会稀释 Wolverine 的优势
- ✅ 按业务能力组织代码，而非技术层次

**理由**:
- Wolverine 的约定机制在切片架构中效果最佳
- 减少跨层跳转，加快开发速度
- 功能变更限制在单个切片内，降低影响范围

#### 原则 2: 一个 Use Case = 一个文件夹

**文件夹结构**:
```
CreateOrder/
├── CreateOrder.cs              # Command 定义
├── CreateOrderEndpoint.cs      # HTTP 端点
├── CreateOrderHandler.cs       # 业务处理器
├── CreateOrderValidator.cs     # 输入验证（可选）
└── OrderCreated.cs            # 领域事件
```

**原则**:
- Command + Handler + Endpoint + Validator + Event 放在一起
- 代码聚合度 > 复用洁癖
- 新人只需打开一个文件夹即可理解完整流程

#### 原则 3: 通信方式分离

| 场景 | 使用方式 | 示例 |
|------|---------|------|
| 同步外部请求 | HTTP Endpoint | 用户点击"开台"按钮 |
| 内部业务编排 | Command Bus | 开台后触发计费初始化 |
| 跨服务异步 | Message Queue | 支付成功后通知订单服务 |

**反模式**:
- ❌ 所有操作都用 Message（过度异步）
- ❌ 所有操作都用 HTTP（紧耦合）

#### 原则 4: Handler 就是 Application Service

**不再需要传统 Service 层**:
```csharp
// ❌ 传统方式
public class TableAppService
{
    public async Task<Guid> StartSession(StartSessionDto dto) { }
}

// ✅ Wolverine 方式
public class StartSessionHandler
{
    public async Task<Result<Guid>> Handle(
        StartSessionCommand command,
        IDocumentSession session)
    {
        // 业务逻辑直接在 Handler 中
    }
}
```

**Handler 是一等公民**:
- 自动依赖注入（方法参数）
- 自动事务管理
- 自动 Unit of Work
- 自动 Outbox 模式

---

## 二、解决方案级 Blueprint

### 2.1 Solution 结构

```text
src/
├── Bootstrapper/                   # 启动 & 组合根
│   ├── Program.cs                  # 应用入口
│   ├── WolverineExtensions.cs      # Wolverine 配置
│   ├── PersistenceExtensions.cs    # Marten/持久化配置
│   └── MessagingExtensions.cs      # 消息传输配置
│
├── Modules/                        # 业务模块（主战场）
│   ├── Tables/                     # 台球桌管理
│   ├── Sessions/                   # 打球时段
│   ├── Orders/                     # 消费订单
│   ├── Payments/                   # 支付对账
│   ├── Members/                    # 会员体系
│   └── Devices/                    # 硬件集成
│
├── BuildingBlocks/                 # 共享基础设施（极度克制）
│   ├── Contracts/                  # 跨模块契约
│   │   ├── IIntegrationEvent.cs    # 集成事件标记接口
│   │   └── Result.cs               # 统一结果类型
│   ├── Behaviors/                  # Wolverine 中间件
│   │   ├── ValidationBehavior.cs   # 验证中间件
│   │   ├── LoggingBehavior.cs      # 日志中间件
│   │   └── TransactionBehavior.cs  # 事务中间件
│   ├── Exceptions/                 # 共享异常类型
│   │   ├── DomainException.cs
│   │   └── NotFoundException.cs
│   └── Clock/                      # 时间抽象
│       ├── IClock.cs
│       └── SystemClock.cs
│
└── Tests/                          # 测试项目
    ├── Tables.Tests/
    ├── Sessions.Tests/
    ├── Payments.Tests/
    └── Integration.Tests/
```

### 2.2 关键设计原则

**Bootstrapper**:
- 唯一职责：启动与组合根
- 不包含业务逻辑
- 扫描并注册所有模块

**Modules**:
- 每个模块代表一个业务能力（Bounded Context）
- 模块间低耦合，通过消息通信
- 模块内高内聚，按功能组织切片

**BuildingBlocks**:
- **极度克制**：只放跨模块不可避免的东西
- 不要创建 `Shared.Core` 大杂烩
- 宁可重复代码，不要过早抽象

---

## 三、单个模块的"黄金结构"

### 3.1 模块目录组织

以 `Tables` 模块为例：

```text
Modules/Tables/
├── Commands/                       # 写操作（可选组织方式）
│   ├── ReserveTable/
│   │   ├── ReserveTable.cs         # Command
│   │   ├── ReserveTableEndpoint.cs # HTTP 端点
│   │   └── ReserveTableHandler.cs  # Handler
│   ├── ReleaseTable/
│   │   ├── ReleaseTable.cs
│   │   └── ReleaseTableHandler.cs
│   └── UpdateTableStatus/
│       ├── UpdateTableStatus.cs
│       └── UpdateTableStatusHandler.cs
│
├── Queries/                        # 读操作（可选组织方式）
│   ├── GetTable/
│   │   ├── GetTable.cs
│   │   ├── GetTableEndpoint.cs
│   │   └── GetTableHandler.cs
│   └── ListTables/
│       ├── ListTables.cs
│       └── ListTablesHandler.cs
│
├── Events/                         # 领域事件
│   ├── TableReserved.cs
│   ├── TableReleased.cs
│   └── TableStatusChanged.cs
│
├── Domain/                         # 模块内领域模型
│   ├── Table.cs                    # 聚合根
│   ├── TableStatus.cs              # 枚举/值对象
│   └── TableType.cs
│
└── TablesModule.cs                 # 模块注册标记
```

### 3.2 替代方案：扁平化组织

如果模块较小，可以采用更扁平的结构：

```text
Modules/Tables/
├── ReserveTable/
│   ├── ReserveTable.cs
│   ├── ReserveTableEndpoint.cs
│   └── ReserveTableHandler.cs
├── ReleaseTable/
│   ├── ReleaseTable.cs
│   └── ReleaseTableHandler.cs
├── GetTable/
│   ├── GetTable.cs
│   └── GetTableHandler.cs
├── Table.cs                        # 聚合根
├── TableStatus.cs
└── TablesModule.cs
```

**选择建议**:
- 功能 < 10 个：使用扁平化
- 功能 > 10 个：按 Commands/Queries 组织
- 有复杂领域模型：增加 Domain 文件夹

---

## 四、完整 Slice 的标准形态

### 4.1 Command 定义

```csharp
namespace Zss.BilliardHall.Modules.Tables.ReserveTable;

/// <summary>
/// 预订台球桌命令
/// </summary>
public sealed record ReserveTable(
    Guid TableId,
    Guid MemberId,
    TimeSpan Duration
);
```

**规范**:
- 使用 `record` 类型（不可变）
- 命名：动词 + 名词（ReserveTable, CreateOrder）
- 只包含必要数据，不包含业务逻辑
- 添加 XML 文档注释

### 4.2 HTTP Endpoint

```csharp
namespace Zss.BilliardHall.Modules.Tables.ReserveTable;

/// <summary>
/// 预订台球桌端点
/// </summary>
public sealed class ReserveTableEndpoint
{
    /// <summary>
    /// 预订台球桌
    /// </summary>
    [WolverinePost("/api/tables/{tableId:guid}/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static ReserveTable Post(
        Guid tableId,
        ReserveTableRequest request
    ) => new(tableId, request.MemberId, request.Duration);

    public sealed record ReserveTableRequest(
        Guid MemberId,
        TimeSpan Duration
    );
}
```

**规范**:
- Endpoint **只做映射**，不写业务逻辑
- 不校验（Wolverine 会触发 Validator）
- 不访问数据库
- 使用 Wolverine 特性标记路由
- 支持路由参数与请求体分离

### 4.3 Handler（核心业务）

```csharp
namespace Zss.BilliardHall.Modules.Tables.ReserveTable;

/// <summary>
/// 预订台球桌处理器
/// </summary>
public sealed class ReserveTableHandler
{
    /// <summary>
    /// 处理预订台球桌命令
    /// </summary>
    /// <remarks>
    /// Wolverine 自动注入 IDocumentSession 和 IMessageBus
    /// </remarks>
    [Transactional]
    public async Task<Result<Guid>> Handle(
        ReserveTable command,
        IDocumentSession session,
        IMessageBus bus,
        CancellationToken ct = default)
    {
        // 1. 加载聚合根
        var table = await session
            .LoadAsync<Table>(command.TableId, ct)
            ?? throw new NotFoundException("台球桌不存在");

        // 2. 业务规则校验
        if (table.Status != TableStatus.Available)
            return Result.Fail<Guid>("台球桌不可用");

        // 3. 执行业务操作
        var reservationId = Guid.NewGuid();
        table.Reserve(reservationId, command.MemberId, command.Duration);

        // 4. 持久化（Marten 自动保存）
        session.Store(table);

        // 5. 发布领域事件
        await bus.PublishAsync(
            new TableReserved(command.TableId, command.MemberId),
            ct
        );

        return Result.Ok(reservationId);
    }
}
```

**Wolverine 的杀手锏**:
- ✅ 自动事务（`[Transactional]` 特性）
- ✅ 自动 Unit of Work
- ✅ 自动 Outbox（事件持久化）
- ✅ 不需要 Repository 接口
- ✅ 不需要手动 `SaveChanges()`

### 4.4 验证器（可选）

```csharp
namespace Zss.BilliardHall.Modules.Tables.ReserveTable;

/// <summary>
/// 预订台球桌验证器
/// </summary>
public sealed class ReserveTableValidator : AbstractValidator<ReserveTable>
{
    public ReserveTableValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty()
            .WithMessage("台球桌ID不能为空");

        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("会员ID不能为空");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("预订时长必须大于0");

        RuleFor(x => x.Duration)
            .LessThanOrEqualTo(TimeSpan.FromHours(8))
            .WithMessage("预订时长不能超过8小时");
    }
}
```

### 4.5 领域事件

```csharp
namespace Zss.BilliardHall.Modules.Tables.Events;

/// <summary>
/// 台球桌已预订事件
/// </summary>
public sealed record TableReserved(
    Guid TableId,
    Guid MemberId,
    DateTimeOffset ReservedAt
)
{
    public TableReserved(Guid tableId, Guid memberId)
        : this(tableId, memberId, DateTimeOffset.UtcNow)
    {
    }
}
```

**事件规范**:
- 命名：名词 + 动词过去式（TableReserved, OrderCreated）
- 表示已发生的事实
- 不可变（record）
- 包含时间戳（UTC）

### 4.6 事件处理器

```csharp
namespace Zss.BilliardHall.Modules.Sessions.Handlers;

/// <summary>
/// 响应台球桌预订事件
/// </summary>
public sealed class TableReservedHandler
{
    /// <summary>
    /// 台球桌预订后自动创建会话
    /// </summary>
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
            Status = SessionStatus.Active
        };

        session.Store(tableSession);
        await session.SaveChangesAsync(ct);

        logger.LogInformation(
            "已为台球桌 {TableId} 创建会话 {SessionId}",
            @event.TableId,
            tableSession.Id
        );
    }
}
```

---

## 五、Saga（跨步骤业务流程）

### 5.1 何时使用 Saga

**适用场景**:
- 业务流程跨多个步骤
- 需要维护状态
- 涉及补偿/回滚逻辑

**自助台球系统典型场景**:
```
开台 → 计时 → 暂停 → 恢复 → 结账 → 支付 → 关台
```

### 5.2 Saga 实现示例

```csharp
namespace Zss.BilliardHall.Modules.Sessions.Sagas;

/// <summary>
/// 打球会话生命周期 Saga
/// </summary>
public sealed class TableSessionSaga : Saga
{
    // Saga 状态
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public SessionStatus Status { get; set; }
    
    /// <summary>
    /// 会话开始
    /// </summary>
    public void Start(SessionStarted @event)
    {
        SessionId = @event.SessionId;
        TableId = @event.TableId;
        StartTime = @event.StartedAt;
        Status = SessionStatus.Active;
    }

    /// <summary>
    /// 会话暂停
    /// </summary>
    public void Pause(SessionPaused @event)
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("只有活跃会话可以暂停");
            
        Status = SessionStatus.Paused;
    }

    /// <summary>
    /// 会话恢复
    /// </summary>
    public void Resume(SessionResumed @event)
    {
        if (Status != SessionStatus.Paused)
            throw new InvalidOperationException("只有暂停会话可以恢复");
            
        Status = SessionStatus.Active;
    }

    /// <summary>
    /// 会话结束（等待支付）
    /// </summary>
    public void End(SessionEnded @event)
    {
        EndTime = @event.EndedAt;
        Status = SessionStatus.PendingPayment;
    }

    /// <summary>
    /// 支付完成（结束 Saga）
    /// </summary>
    public void Complete(PaymentCompleted @event)
    {
        if (@event.SessionId != SessionId)
            return;
            
        Status = SessionStatus.Completed;
        
        // 标记 Saga 完成
        MarkCompleted();
    }
}
```

**Saga 配置**:
```csharp
// 在 Program.cs 中配置
builder.Host.UseWolverine(opts =>
{
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    
    // 启用 Saga 持久化
    opts.Services.AddMarten(marten =>
    {
        marten.Connection(connectionString);
        
        // 注册 Saga
        marten.Schema.For<TableSessionSaga>()
            .Identity(x => x.SessionId);
    });
});
```

---

## 六、跨模块通信（底线规则）

### 6.1 通信方式选择

| 场景 | 方式 | 工具 | 示例 |
|------|------|------|------|
| 同模块内 | 直接方法调用 | - | Handler 内调用领域服务 |
| 跨模块同步 | Command Bus | `IMessageBus.InvokeAsync()` | 结账时触发计费 |
| 跨模块异步 | Event | `IMessageBus.PublishAsync()` | 会员注册后发送欢迎邮件 |
| 跨服务 | Message Queue | RabbitMQ/Kafka | 支付完成通知第三方系统 |

### 6.2 反模式：共享服务层

❌ **错误示范**:
```csharp
// 不要创建 Shared Service
public class SharedBillingService
{
    public decimal CalculatePrice(TimeSpan duration) { }
}

// 多个模块都依赖它
public class EndSessionHandler
{
    private readonly SharedBillingService _billingService;
}
```

✅ **正确做法**:
```csharp
// 通过命令/查询通信
public class EndSessionHandler
{
    public async Task Handle(
        EndSession command,
        IMessageBus bus)
    {
        // 发送命令到 Billing 模块
        var bill = await bus.InvokeAsync<BillResult>(
            new CalculateBill(sessionId, duration)
        );
    }
}
```

**原则**:
- Shared Service = 隐形耦合 = 架构腐烂起点
- 宁可通过消息通信，也不创建共享服务
- 真正的核心领域逻辑（如定价算法）可以例外

---

## 七、项目现实映射（自助台球系统）

### 7.1 推荐模块拆分

```text
Modules/
├── Tables/          # 台球桌生命周期（开台/关台/状态管理）
│   ├── ReserveTable/
│   ├── ReleaseTable/
│   ├── GetTable/
│   └── ListTables/
│
├── Sessions/        # 打球时段（计时/暂停/续费）
│   ├── StartSession/
│   ├── EndSession/
│   ├── PauseSession/
│   ├── ResumeSession/
│   └── Sagas/
│       └── TableSessionSaga.cs
│
├── Billing/         # 计费（价格计算/账单生成）
│   ├── CalculateBill/
│   ├── GenerateInvoice/
│   └── ApplyDiscount/
│
├── Payments/        # 支付 & 对账
│   ├── ProcessPayment/
│   ├── RefundPayment/
│   ├── ReconcilePayments/
│   └── GetPaymentHistory/
│
├── Members/         # 会员体系
│   ├── RegisterMember/
│   ├── TopUpBalance/
│   ├── GetMemberProfile/
│   └── UpdateMemberTier/
│
└── Devices/         # 门禁/灯控/硬件
    ├── ControlDoorLock/
    ├── ControlLighting/
    └── GetDeviceStatus/
```

### 7.2 不推荐的拆分

❌ **过度拆分**:
```text
Modules/
├── TableReservation/        # 太细粒度
├── TableRelease/
├── TableStatusQuery/
```

❌ **过度合并**:
```text
Modules/
├── Billing/                 # 太大，职责混乱
│   ├── Sessions/
│   ├── Payments/
│   └── Invoices/
```

❌ **技术拆分**:
```text
Modules/
├── Commands/                # 按技术层拆分，错误！
├── Queries/
└── Events/
```

### 7.3 模块边界判定

**一个模块的判定标准**:
1. 有清晰的业务能力边界（Tables 管理桌台，Sessions 管理时段）
2. 可以独立演化（修改 Sessions 不影响 Payments）
3. 有自己的数据模型（Table、Session、Payment 是不同实体）
4. 团队可以独立工作（不同开发者负责不同模块）

---

## 八、硬核实践建议

### 8.1 先照抄，再优化

**第一阶段：照抄官方 Sample**
```csharp
// 直接使用官方示例的风格
public class CreateOrderHandler
{
    public async Task<Guid> Handle(
        CreateOrder command,
        IDocumentSession session)
    {
        var order = new Order { /* ... */ };
        session.Store(order);
        await session.SaveChangesAsync();
        return order.Id;
    }
}
```

**第二阶段：根据项目调整**
```csharp
// 加入项目规范（如 Result 类型）
public class CreateOrderHandler
{
    public async Task<Result<Guid>> Handle(
        CreateOrder command,
        IDocumentSession session)
    {
        // 校验逻辑
        if (command.Amount <= 0)
            return Result.Fail<Guid>("金额必须大于0");

        var order = new Order { /* ... */ };
        session.Store(order);
        await session.SaveChangesAsync();
        return Result.Ok(order.Id);
    }
}
```

### 8.2 不要"重构洁癖"

**反模式：过早抽象**
```csharp
// ❌ 只有 2 个 Handler 就开始抽象
public abstract class BaseHandler<TCommand, TResult>
{
    protected abstract Task<TResult> ExecuteAsync(TCommand command);
}
```

**正确做法：接受重复**
```csharp
// ✅ 允许适度重复
public class CreateOrderHandler
{
    public async Task<Guid> Handle(CreateOrder cmd, IDocumentSession session)
    {
        // 直接实现，不抽象
    }
}

public class CreateMemberHandler
{
    public async Task<Guid> Handle(CreateMember cmd, IDocumentSession session)
    {
        // 即使有相似逻辑，也不要过早抽象
    }
}
```

**经验法则**:
- 重复 2 次：观察
- 重复 3 次：考虑抽象
- 重复 5 次：必须抽象

### 8.3 宁可重复代码，也别提前抽象

**示例场景：多个模块都需要发送通知**

❌ **过早抽象**:
```csharp
// 创建共享通知服务
public interface INotificationService
{
    Task SendAsync(string template, object data);
}

// 多个模块依赖
public class CreateOrderHandler
{
    private readonly INotificationService _notification;
}
```

✅ **接受重复**:
```csharp
// 每个模块独立实现
// Orders 模块
public class OrderCreatedHandler
{
    public async Task Handle(OrderCreated evt, IEmailService email)
    {
        await email.SendAsync("order-created", new { evt.OrderId });
    }
}

// Members 模块
public class MemberRegisteredHandler
{
    public async Task Handle(MemberRegistered evt, IEmailService email)
    {
        await email.SendAsync("member-welcome", new { evt.MemberId });
    }
}
```

**何时抽象**:
- 当通知逻辑变得复杂（模板管理、多渠道）
- 当需要统一审计/监控
- 当业务要求统一行为（如失败重试策略）

### 8.4 Handler 复杂度信号

**警告信号**:
- Handler 超过 50 行
- 包含多个 if/else 分支
- 需要注入 5+ 个依赖

**解决方案**:
1. **拆分 Handler**: 将一个大 Command 拆成多个小 Command
2. **引入领域服务**: 复杂逻辑下沉到领域层
3. **使用 Saga**: 跨步骤流程用 Saga 编排

**示例**:
```csharp
// ❌ Handler 太复杂
public class ProcessOrderHandler
{
    public async Task Handle(ProcessOrder cmd)
    {
        // 50+ 行代码
        // 包含库存检查、价格计算、优惠券、积分...
    }
}

// ✅ 拆分成多个步骤
public class ProcessOrderSaga : Saga
{
    public void Handle(OrderCreated evt) => /* 触发库存检查 */;
    public void Handle(StockReserved evt) => /* 触发价格计算 */;
    public void Handle(PriceCalculated evt) => /* 触发支付 */;
}
```

### 8.5 测试策略

**单元测试（隔离 Handler）**:
```csharp
public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Should_Create_Order_Successfully()
    {
        // 使用 In-Memory Marten
        await using var store = DocumentStore.For(opts =>
        {
            opts.Connection(ConnectionSource.InMemoryConnectionString);
        });
        
        await using var session = store.LightweightSession();
        
        var handler = new CreateOrderHandler();
        var command = new CreateOrder(/* ... */);
        
        var result = await handler.Handle(command, session);
        
        result.IsSuccess.ShouldBeTrue();
    }
}
```

**集成测试（完整流程）**:
```csharp
public class OrderFlowTests : IAsyncLifetime
{
    private IHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.Services.AddMarten(/* test DB */);
            })
            .StartAsync();
    }

    [Fact]
    public async Task Should_Complete_Order_Flow()
    {
        var bus = _host.Services.GetRequiredService<IMessageBus>();
        
        // 创建订单
        var orderId = await bus.InvokeAsync<Guid>(
            new CreateOrder(/* ... */)
        );
        
        // 处理支付
        await bus.InvokeAsync(new ProcessPayment(orderId, 100m));
        
        // 验证结果
        var session = _host.Services.GetRequiredService<IDocumentSession>();
        var order = await session.LoadAsync<Order>(orderId);
        order.Status.ShouldBe(OrderStatus.Completed);
    }
}
```

---

## 九、Bootstrapper 配置示例

### 9.1 Program.cs

```csharp
using Wolverine;
using Marten;

var builder = WebApplication.CreateBuilder(args);

// 添加 Marten
builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("Postgres")!;
    
    opts.Connection(connectionString);
    
    // 自动创建数据库
    opts.AutoCreateSchemaObjects = AutoCreate.All;
    
    // 注册实体
    opts.Schema.For<Table>().Index(x => x.Status);
    opts.Schema.For<TableSession>().Index(x => x.TableId);
    opts.Schema.For<Member>().UniqueIndex(x => x.Phone);
    
    // 集成 Wolverine
    opts.IntegrateWithWolverine();
});

// 添加 Wolverine
builder.Host.UseWolverine(opts =>
{
    // 持久化消息
    opts.PersistMessagesWithMarten();
    
    // 自动发现处理器（扫描所有模块）
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    
    // 配置本地队列
    opts.LocalQueue("billing")
        .UseDurableInbox()
        .Sequential();
    
    opts.LocalQueue("notifications")
        .UseDurableInbox()
        .MaximumParallelMessages(5);
    
    // 全局策略
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();
    
    // 重试策略
    opts.Policies.OnException<HttpRequestException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds());
});

// 添加健康检查
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!);

var app = builder.Build();

// 映射 Wolverine 端点
app.MapWolverineEndpoints();

// 健康检查
app.MapHealthChecks("/health");

app.Run();
```

### 9.2 模块化配置（可选）

```csharp
// WolverineExtensions.cs
public static class WolverineExtensions
{
    public static IHostBuilder AddWolverineModules(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        return host.UseWolverine(opts =>
        {
            opts.PersistMessagesWithMarten();
            opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
            
            // Tables 模块配置
            opts.PublishMessage<TableReserved>()
                .ToLocalQueue("sessions");
            
            // Payments 模块配置
            opts.PublishMessage<PaymentCompleted>()
                .ToLocalQueue("billing");
            
            // 如果需要 RabbitMQ
            if (configuration.GetValue<bool>("UseRabbitMQ"))
            {
                opts.UseRabbitMq(rabbit =>
                {
                    rabbit.HostName = configuration["RabbitMQ:Host"]!;
                    rabbit.AutoProvision();
                });
                
                opts.PublishMessage<PaymentCompleted>()
                    .ToRabbitQueue("external-payment-events");
            }
        });
    }
}

// Program.cs 中使用
builder.Host.AddWolverineModules(builder.Configuration);
```

---

## 十、FAQ

### Q1: Wolverine 与 MediatR 如何选择？

**选择 Wolverine**:
- 需要消息持久化（Outbox 模式）
- 需要跨进程通信（RabbitMQ/Kafka）
- 需要后台任务/定时任务
- 需要 Saga/工作流
- 团队愿意学习约定

**选择 MediatR**:
- 只需要简单的 CQRS 进程内消息
- 团队已熟悉 MediatR
- 不需要持久化和跨进程通信

### Q2: 是否可以混合使用 ABP 分层？

**不推荐**，理由：
- Wolverine 的优势在垂直切片中最大化
- 混合使用会导致架构混乱
- 团队认知负担增加

**迁移策略**:
1. 新功能用 Wolverine + 垂直切片
2. 老功能逐步迁移（非强制）
3. 保持 Domain 层的领域模型（可共享）

### Q3: 如何处理跨模块的实体关系？

**方案 1：通过 ID 关联（推荐）**
```csharp
// Sessions 模块
public class TableSession
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }  // 只存 ID，不存对象
    public Guid MemberId { get; set; }
}

// 需要完整数据时，通过消息查询
var table = await bus.InvokeAsync<Table>(new GetTable(session.TableId));
```

**方案 2：数据冗余（特定场景）**
```csharp
// 为了性能，可以冗余少量只读数据
public class TableSession
{
    public Guid TableId { get; set; }
    public string TableName { get; set; }  // 冗余数据，仅用于展示
}
```

**方案 3：视图/投影（CQRS）**
```csharp
// 创建专门的查询视图
public class SessionSummaryView
{
    public Guid SessionId { get; set; }
    public string TableName { get; set; }
    public string MemberName { get; set; }
    // ... 聚合多个模块的数据
}
```

### Q4: 如何测试 Wolverine Handler？

见"八、硬核实践建议 > 8.5 测试策略"

---

## 十一、参考资源

### 官方文档
- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Marten Documentation](https://martendb.io/)
- [Vertical Slice Architecture - Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)

### 示例项目
- [Wolverine Samples](https://github.com/JasperFx/wolverine/tree/main/samples)
- [Marten Samples](https://github.com/JasperFx/marten/tree/master/samples)

### 推荐阅读
- [Feature Folders](https://www.youtube.com/watch?v=yF6VL35l914)
- [CQRS Journey](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))

---

## 十二、版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0.0 | 2024-01-15 | 初始版本，完整蓝图 |

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
