# Wolverine 模块化架构蓝图

> **定位**: 可直接落地、非 PPT 架构的实战型 Wolverine 模块化规划
> 
> **适用场景**: 自助台球系统等业务能力清晰的中小型应用
>
> **核心观点**: Wolverine ≠ MediatR 替代品。它是 HTTP + Command Bus + Message Bus + Workflow 引擎的融合体
>
> **框架简介**: Wolverine 是现代化的 .NET 应用框架，专注于简化消息处理、命令处理和后台任务，是 [JasperFx](https://github.com/JasperFx) 生态系统的一部分，与 Marten 紧密集成。[官方文档](https://wolverine.netlify.app/)

---

## 序章：垂直切片架构理念

### 什么是垂直切片架构？

垂直切片架构（Vertical Slice Architecture）是一种以功能特性为中心的软件架构模式，每个功能切片包含从 UI 到数据库的完整实现路径。与传统的水平分层架构（如 ABP 的 Domain/Application/HttpApi/Infrastructure）不同，垂直切片架构强调**按业务功能组织代码，而非技术层次**。

### 核心理念对比

```
传统分层架构：              垂直切片架构：
Controller/                Modules/
  UserController             Members/
  TableController              RegisterMember/
Application/                     RegisterMember.cs (Command)
  UserService                    RegisterMemberHandler.cs
  TableService                   RegisterMemberValidator.cs
Domain/                          RegisterMemberEndpoint.cs
  User                         TopUpBalance/
  Table                          TopUpBalance.cs
Infrastructure/                  TopUpBalanceHandler.cs
  UserRepository              Tables/
  TableRepository               ReserveTable/
                                  ReserveTableCommand.cs
                                  ReserveTableHandler.cs
                                ReleaseTable/
                                  ReleaseTableCommand.cs
                                  ReleaseTableHandler.cs
```

### 为什么选择垂直切片？

**高内聚、低耦合**:
- **每个切片独立**: 一个业务功能的所有代码（处理器、验证、数据访问）都在同一个切片中
- **最小化跨切片依赖**: 不同切片之间通过消息、事件通信
- **独立演化**: 每个切片可以独立修改、测试和部署

**以业务为中心**:
- 按业务能力组织代码（Members/Tables/Billing），而非技术层次
- 新人只需打开一个切片文件夹即可理解完整业务流程
- 功能变更限制在单个切片内，降低影响范围

### 与传统分层架构的对比

| 维度 | 传统分层架构 | 垂直切片架构 |
|------|-------------|-------------|
| 代码组织 | 按技术层次（Controller/Service/Repository） | 按业务功能（Features/Users/Tables） |
| 依赖方向 | 单向向下（UI → Application → Domain → Infrastructure） | 每个切片独立，通过消息通信 |
| 变更影响 | 一个功能改动可能涉及多层 | 功能改动集中在单个切片内 |
| 测试策略 | 需要 Mock 多层依赖 | 切片可独立集成测试 |
| 新人理解 | 需要理解完整分层结构 | 只需理解单个切片即可开始 |
| 代码复用 | 通过共享服务层实现 | 接受适度重复，共享真正通用的 |
| 学习曲线 | 需要理解分层边界和职责 | 聚焦业务流程，技术细节透明 |

### Wolverine 与垂直切片的完美结合

Wolverine 的**约定优于配置**理念与垂直切片架构天然契合：

1. **自动发现机制**: Wolverine 自动发现和注册 Handler，无需手动配置
2. **方法参数注入**: 依赖直接注入到 Handler 方法参数，无需构造函数注入
3. **级联消息**: Handler 返回值自动作为消息发布，简化跨切片通信
4. **事务管理**: `[Transactional]` 特性自动管理事务，无需手动 SaveChanges
5. **Outbox 模式**: 与 Marten 集成，保证消息和数据的一致性

这些特性使得在垂直切片架构中，每个切片的代码更加简洁、独立和易于理解。

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

### 2.3 BuildingBlocks 防污染铁律 ⚠️

> **警告**: 99% 的团队会在 BuildingBlocks 上失败。这是防腐层，不是工具箱！

**进入 BuildingBlocks 的严格条件**:

✅ **必须满足以下所有条件**:
1. **被 3 个以上模块真实使用**（不是"将来可能用"）
2. **跨模块不可避免**（不能通过消息通信解决）
3. **没有业务语义**（纯技术设施）
4. **不会频繁变更**（稳定的契约）
5. **抽象后修改成本真的降低**（隐含条件，见下方说明）

> ⚠️ **残酷的真相**: "3 个模块使用" ≠ "值得抽象"
> 
> 如果 3 个模块都 copy 了同一段**烂设计**，抽取后的结果是：
> - 谁都不敢改（变成祖宗牌位）
> - 业务一变，BuildingBlocks 反而成为枷锁
> 
> **判断标准**：抽象之后，修改成本是否真的下降？
> - 如果答案是否定的，哪怕 5 个模块在用，也不要抽
> - 宁可重复，不要错误的抽象

❌ **禁止进入的示例**:
- 只有 1-2 个模块使用的工具类
- 包含业务规则的共享服务
- "万能" Helper/Util 类
- 特定业务领域的枚举/常量

**正确做法**:
```csharp
// ❌ 错误：只有 2 个模块用就抽取
// BuildingBlocks/Helpers/PriceCalculator.cs
public static class PriceCalculator
{
    public static decimal Calculate(TimeSpan duration) { }
}

// ✅ 正确：在各自模块内复制
// Modules/Billing/PriceCalculator.cs
internal static class PriceCalculator
{
    public static decimal Calculate(TimeSpan duration) { }
}

// Modules/Sessions/PriceEstimator.cs
internal static class PriceEstimator
{
    public static decimal Estimate(TimeSpan duration) { }
}
```

**决策流程**:
```
需要共享代码？
  ├─ 是否被 3+ 模块使用？
  │   ├─ 否 → 复制代码到各模块
  │   └─ 是 → 是否纯技术设施？
  │       ├─ 否 → 通过事件/命令通信
  │       └─ 是 → 可进入 BuildingBlocks
  └─ 否 → 保持在模块内
```

**审核检查清单**:
- [ ] 提供 3 个真实使用场景（不是假设）
- [ ] 证明无法通过消息通信解决
- [ ] 确认不包含业务语义
- [ ] 评估变更频率（每月 < 1 次）
- [ ] 评估抽象后是否真的降低修改成本

**⚠️ ErrorCodes 的高级陷阱**:

`ErrorCodes` 是最容易被滥用的 BuildingBlocks 组件，因为它**太好用**。

**风险**: 一旦 ErrorCodes 开始承载业务语义，就等于把业务规则偷偷搬进 BuildingBlocks。

**铁律**: ErrorCodes 只允许表达"失败类型"，不允许表达"业务决策原因"

```csharp
// ✅ 好的：表达失败类型
ErrorCodes.Tables.NotFound
ErrorCodes.Tables.InvalidStatus
ErrorCodes.Tables.Conflict
ErrorCodes.Tables.Forbidden

// ❌ 危险的：表达业务决策原因
ErrorCodes.Tables.CannotReserveAtNight      // 业务规则！
ErrorCodes.Members.MemberLevelTooLow        // 业务规则！
ErrorCodes.Billing.PromotionExpired         // 业务规则！
```

**正确做法**: 业务决策相关的错误码必须在模块内定义

```csharp
// Modules/Tables/ErrorCodes.cs (模块内部)
internal static class TableErrorCodes
{
    public const string CannotReserveAtNight = "Tables:CannotReserveAtNight";
}
```

**防护建议**:
- Code Review 时严格审查 ErrorCodes 新增项
- 问自己：这是"技术失败"还是"业务决策"？
- 即使字符串重复，也认了（重复好过错误的抽象）

### 2.4 事件分类与边界管理 ⚠️

> **核心问题**: 不明确的事件边界是微服务和模块化项目最常见的腐烂点

**必须明确的三种事件层级**:

| 事件类型 | 范围 | 是否跨模块 | 存放位置 | 可修改性 |
|---------|------|-----------|---------|---------|
| **Domain Event** | 模块内 | ❌ | `Modules/{Module}/Events/` | ✅ 可自由修改 |
| **Module Event** | 本进程跨模块 | ⚠️ | `Modules/{Module}/Events/` | ⚠️ 需考虑消费者 |
| **Integration Event** | 跨服务 | ✅ | `BuildingBlocks/Contracts/` | ❌ 严格版本管理 |

**事件命名与组织**:

```text
# Domain Event（模块内部）
Modules/Tables/Events/
├── TableReserved.cs              # 仅在 Tables 模块内消费
└── TableStatusChanged.cs         # 内部状态机事件

# Module Event（跨模块）
Modules/Sessions/Events/
├── SessionStarted.cs             # 可能被 Billing/Devices 模块消费
└── SessionEnded.cs

# Integration Event（跨服务）
BuildingBlocks/Contracts/IntegrationEvents/
├── PaymentCompletedIntegrationEvent.cs    # 通知外部支付系统
└── MemberRegisteredIntegrationEvent.cs    # 同步到CRM系统
```

**事件升级路径**:

```
Domain Event (模块内)
    ↓ 有其他模块需要监听
Module Event (跨模块)
    ↓ 需要通知外部服务
Integration Event (跨服务)
```

**⚠️ Module Event 的特殊风险**:

> **现实真相**: Module Event 是最容易被"随便用"的事件层级

原因：
- 不用管版本
- 不用管外部系统
- "反正都在一个进程里"

结果：
- A 模块偷偷监听 B 的内部演进事件
- B 模块一改，A 爆炸

**强制要求**: Module Event 必须被显式声明为"对外事件"

**推荐做法**:

1. **文件夹命名区分**:
```text
Modules/Sessions/
├── Events/                    # 内部事件（Domain Event）
│   └── SessionStateChanged.cs
└── PublicEvents/              # 对外事件（Module Event）
    ├── SessionStarted.cs
    └── SessionEnded.cs
```

2. **或使用注释/Attribute 标记**:
```csharp
/// <summary>
/// 会话开始事件（Module Event - 对外契约）
/// </summary>
/// <remarks>
/// 消费者：
/// - Billing 模块（计费开始）
/// - Devices 模块（设备控制）
/// 修改此事件需通知所有消费方
/// </remarks>
public sealed record SessionStarted(...);
```

**目的**: 让作者在创建时意识到——这是契约，不是内部玩具

**反模式与风险**:

❌ **错误 1：事件混放**
```csharp
// ❌ 所有事件都放在 Events 文件夹，没有区分
Modules/Tables/Events/
├── TableReserved.cs              // 不知道是否跨模块
├── PaymentCompleted.cs           // 不知道是否跨服务
```

❌ **错误 2：Integration Event 放在模块内**
```csharp
// ❌ 跨服务事件不应该在模块内
Modules/Payments/Events/
└── PaymentCompletedIntegrationEvent.cs  // 应该在 BuildingBlocks/Contracts
```

✅ **正确做法**:
```csharp
// Domain Event（内部）
namespace Zss.BilliardHall.Modules.Tables.Events;

internal sealed record TableStatusChanged(
    Guid TableId,
    TableStatus OldStatus,
    TableStatus NewStatus
);

// Module Event（跨模块）
namespace Zss.BilliardHall.Modules.Sessions.Events;

public sealed record SessionStarted(
    Guid SessionId,
    Guid TableId,
    Guid MemberId,
    DateTimeOffset StartedAt
);

// Integration Event（跨服务）
namespace Zss.BilliardHall.BuildingBlocks.Contracts.IntegrationEvents;

public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTimeOffset CompletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

**事件修改影响分析**:

```
Domain Event 修改
  → 影响范围：单个模块
  → 风险等级：低
  → 审批要求：无

Module Event 修改
  → 影响范围：依赖模块
  → 风险等级：中
  → 审批要求：通知消费方

Integration Event 修改
  → 影响范围：所有服务
  → 风险等级：高
  → 审批要求：版本升级 + 兼容性保证
  
**⚠️ Integration Event 不可修改铁律**:

> **一旦发布，视为"只增不改"**

现实规则：
- ❌ **不改**字段含义
- ❌ **不删**字段
- ✅ **只能加**字段（可选）
- ⚠️ **老字段哪怕废弃也要留**

否则后续会反噬：
- Kafka / RabbitMQ 历史消息
- Outbox 重放
- 跨服务版本不一致
- 回滚困难

**正确的演进方式**:
```csharp
// V1 - 初始版本
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    decimal Amount
) : IIntegrationEvent;

// V2 - 新增字段（向后兼容）
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    decimal Amount,
    string? Currency = "CNY"  // 新增可选字段
) : IIntegrationEvent;

// ❌ 错误：删除或修改字段含义
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    decimal Amount  // 改为含税金额 - 破坏兼容性！
) : IIntegrationEvent;
```
```

**检查清单**:
- [ ] 新事件是否明确分类（Domain/Module/Integration）
- [ ] Integration Event 是否实现 `IIntegrationEvent`
- [ ] Module Event 是否有明确的消费者文档
- [ ] 事件修改是否评估了影响范围

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

### 3.3 模块标记（Module Marker）

**每个模块必须有显式的 Module Marker**:

```csharp
namespace Zss.BilliardHall.Modules.Tables;

/// <summary>
/// Tables 模块标记
/// </summary>
/// <remarks>
/// 职责：
/// 1. 模块身份标识（用于自动扫描）
/// 2. 权限边界管理（模块级授权）
/// 3. Feature Toggle 配置
/// 4. 模块级日志与追踪
/// </remarks>
public sealed class TablesModule : IWolverineModule
{
    public static string ModuleName => "Tables";
}
```

**Module Marker 的三个核心职责**:

**1. 自动模块扫描**:
```csharp
// Program.cs
builder.Host.UseWolverine(opts =>
{
    // 自动发现所有实现 IWolverineModule 的模块
    var moduleTypes = typeof(Program).Assembly
        .GetTypes()
        .Where(t => typeof(IWolverineModule).IsAssignableFrom(t));
    
    foreach (var moduleType in moduleTypes)
    {
        var moduleName = moduleType.GetProperty("ModuleName")?.GetValue(null);
        Console.WriteLine($"Discovered module: {moduleName}");
    }
});
```

**2. 权限边界管理**:
```csharp
// 基于模块的授权策略
public class ModuleAuthorizationHandler
{
    public bool CanAccess(string moduleName, ClaimsPrincipal user)
    {
        // 检查用户是否有访问特定模块的权限
        return user.HasClaim("module", moduleName);
    }
}
```

**3. Feature Toggle 配置**:
```csharp
// appsettings.json
{
    "FeatureToggles": {
        "Tables": true,
        "Payments": true,
        "Devices": false  // 硬件模块暂时禁用
    }
}

// 使用
if (configuration.GetValue<bool>($"FeatureToggles:{TablesModule.ModuleName}"))
{
    // 启用 Tables 模块
}
```

**4. 模块级日志**:
```csharp
public class ReserveTableHandler
{
    public async Task<Result<Guid>> Handle(
        ReserveTable command,
        ILogger<ReserveTableHandler> logger)
    {
        logger.LogInformation(
            "[{Module}] 处理预订命令 {TableId}",
            TablesModule.ModuleName,
            command.TableId
        );
    }
}
```

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

> **⚠️ 警告**: Saga 是重武器，不是常规武器！误用会导致"状态机地狱"

> **💡 心理刹车**: 如果你在犹豫要不要用 Saga，答案通常是：**不要**

### 5.1 何时使用 Saga（收紧标准）

**Saga 使用的 3 条铁律（必须全部满足）**:

1. ✅ **跨模块**：涉及 2 个以上模块的协作
2. ✅ **跨时间**：流程持续时间 > 1 分钟（需要等待外部事件）
3. ✅ **需要补偿**：失败时需要补偿而不是简单回滚

**决策树**:

```
需要编排多个步骤？
  ├─ 是否跨模块？
  │   ├─ 否 → 使用普通 Handler
  │   └─ 是 → 是否跨时间（> 1分钟）？
  │       ├─ 否 → 使用 Command 链（InvokeAsync）
  │       └─ 是 → 是否需要补偿？
  │           ├─ 否 → 使用事件驱动（PublishAsync）
  │           └─ 是 → ✅ 使用 Saga
  └─ 否 → 单步操作，无需 Saga
```

**✅ 适合 Saga 的场景**:

```
场景 1：订单处理流程
  订单创建 → 库存锁定（等待） → 支付（等待用户） → 发货
  ✅ 跨模块：Orders + Inventory + Payments + Shipping
  ✅ 跨时间：等待用户支付（分钟到小时级）
  ✅ 需要补偿：支付失败需要释放库存

场景 2：打球会话生命周期
  开台 → 计时（等待） → 暂停/恢复 → 结账（等待） → 支付 → 关台
  ✅ 跨模块：Sessions + Tables + Billing + Payments
  ✅ 跨时间：用户打球时间（小时级）
  ✅ 需要补偿：支付失败需要恢复会话或标记欠费
```

**❌ 不适合 Saga 的场景**:

```
场景 1：用户注册后发送欢迎邮件
  ❌ 不跨时间：即时操作
  ❌ 无需补偿：邮件发送失败不影响注册
  → 解决方案：使用事件（PublishAsync）

场景 2：创建订单并初始化库存
  ❌ 不跨时间：同步完成
  ❌ 无需补偿：事务内原子操作
  → 解决方案：在 Handler 内直接调用（InvokeAsync）

场景 3：查询聚合数据
  ❌ 无状态：纯查询操作
  → 解决方案：使用投影/视图
```

**常见误用**:

```csharp
// ❌ 错误：只是为了拆分代码而用 Saga
public class CreateOrderSaga : Saga
{
    public void Handle(CreateOrderCommand cmd)
    {
        // 这只是一个普通的命令处理
        // 不需要 Saga！
    }
}

// ✅ 正确：直接用 Handler
public class CreateOrderHandler
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd)
    {
        // 简单直接
    }
}
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

### 6.2 跨进程同步命令的铁律 ⚠️

> **核心原则**: `InvokeAsync` 只能用于进程内模块通信

**✅ 允许（进程内）**:
```csharp
// ✅ 同一个应用内的不同模块
public class EndSessionHandler
{
    public async Task Handle(EndSession command, IMessageBus bus)
    {
        // Billing 模块在同一进程
        var bill = await bus.InvokeAsync<BillResult>(
            new CalculateBill(sessionId, duration)
        );
    }
}
```

**❌ 禁止（跨服务）**:
```csharp
// ❌ 跨服务同步调用 - 紧耦合 + 性能风险
public class OrderHandler
{
    public async Task Handle(CreateOrder cmd, IMessageBus bus)
    {
        // 错误：尝试同步调用外部支付服务
        var result = await bus.InvokeAsync<PaymentResult>(
            new ProcessExternalPayment(...)  // 外部服务！
        );
    }
}
```

**跨服务通信的正确方式**:

**方式 1：事件驱动（推荐）**
```csharp
// ✅ 发布事件，外部服务订阅
public class OrderHandler
{
    public async Task Handle(CreateOrder cmd, IMessageBus bus)
    {
        // 创建订单
        var order = new Order { /* ... */ };
        
        // 发布事件（异步）
        await bus.PublishAsync(new OrderCreatedIntegrationEvent(order.Id));
        
        // 不等待外部服务响应
        return Result.Ok(order.Id);
    }
}

// 外部支付服务监听事件
public class PaymentServiceListener
{
    public async Task Handle(OrderCreatedIntegrationEvent evt)
    {
        // 处理支付
    }
}
```

**方式 2：补偿模式（Saga）**
```csharp
// ✅ 使用 Saga 编排跨服务流程
public class OrderSaga : Saga
{
    public async Task Handle(OrderCreated evt)
    {
        // 发起外部支付请求
        await PublishAsync(new RequestPayment(evt.OrderId));
    }
    
    // 等待外部服务回调
    public void Handle(PaymentCompleted evt)
    {
        // 继续流程
    }
    
    // 超时补偿
    public void Handle(PaymentTimeout evt)
    {
        // 取消订单
    }
}
```

**方式 3：HTTP API（显式远程调用）**
```csharp
// ✅ 如果必须同步调用，使用明确的 HTTP 客户端
public class OrderHandler
{
    private readonly IPaymentServiceClient _paymentClient;
    
    public async Task Handle(CreateOrder cmd)
    {
        // 明确这是远程调用（不要伪装成本地消息）
        var result = await _paymentClient.ProcessPaymentAsync(...);
    }
}
```

**防护措施**:
```csharp
// 在消息配置中明确区分本地和远程
builder.Host.UseWolverine(opts =>
{
    // ✅ 本地队列 - 允许 InvokeAsync
    opts.LocalQueue("billing").UseDurableInbox();
    
    // ❌ 外部队列 - 禁止 InvokeAsync（只能 PublishAsync）
    opts.PublishMessage<OrderCreated>()
        .ToRabbitQueue("external-orders")
        .UseDurableOutbox();
});
```

**检查清单**:
- [ ] `InvokeAsync` 调用的目标是否在同一进程？
- [ ] 跨服务调用是否使用事件或 HTTP？
- [ ] 是否避免了"伪装成本地消息的远程调用"？

### 6.3 反模式：共享服务层

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

**第三阶段：引入结构化错误码（推荐）**
```csharp
// 使用 ErrorCode 实现错误聚合
public class CreateOrderHandler
{
    public async Task<Result<Guid>> Handle(
        CreateOrder command,
        IDocumentSession session)
    {
        // 使用结构化错误码：Area:Key
        if (command.Amount <= 0)
            return Result.Fail<Guid>(
                "金额必须大于0",
                "Orders:InvalidAmount"  // 可被前端识别、日志聚合
            );

        var order = new Order { /* ... */ };
        session.Store(order);
        await session.SaveChangesAsync();
        return Result.Ok(order.Id);
    }
}
```

### 8.2 Result<T> 错误模型管理 ⚠️

> **警告**: 不收敛错误模型，6 个月后会后悔

**Result<T> 失控的症状**:
```csharp
// ❌ 错误消息无规律
Result.Fail<Guid>("xxx")
Result.Fail<Guid>("台球桌不可用")
Result.Fail<Guid>("Table unavailable")
Result.Fail<Guid>("桌子正忙")  // 同一个错误，不同描述

// 问题：
// 1. 前端无法区分错误类型
// 2. 日志无法聚合统计
// 3. 重试策略无法生效
// 4. 多语言支持困难
```

**解决方案：结构化错误码**

**错误码规范**:
- 格式：`{Area}:{Key}`
- Area：模块名或功能域
- Key：具体错误类型（PascalCase）

```csharp
// ✅ 结构化错误码示例
"Billing:TableUnavailable"      // 计费模块：台球桌不可用
"Billing:InsufficientBalance"   // 计费模块：余额不足
"Sessions:SessionNotFound"      // 会话模块：会话不存在
"Sessions:SessionAlreadyEnded"  // 会话模块：会话已结束
"Payments:PaymentFailed"        // 支付模块：支付失败
"Payments:RefundNotAllowed"     // 支付模块：不允许退款
```

**实现示例**:

**方式 1：使用 Result.ErrorCode**
```csharp
public class ReserveTableHandler
{
    public async Task<Result<Guid>> Handle(
        ReserveTable command,
        IDocumentSession session)
    {
        var table = await session.LoadAsync<Table>(command.TableId);
        
        if (table == null)
            return Result.Fail<Guid>(
                "台球桌不存在",
                "Tables:NotFound"
            );
        
        if (table.Status != TableStatus.Available)
            return Result.Fail<Guid>(
                "台球桌不可用",
                "Tables:Unavailable"
            );
        
        // 成功逻辑...
        return Result.Ok(reservationId);
    }
}
```

**方式 2：错误码常量类（推荐）**
```csharp
// BuildingBlocks/Contracts/ErrorCodes.cs
public static class ErrorCodes
{
    public static class Tables
    {
        public const string NotFound = "Tables:NotFound";
        public const string Unavailable = "Tables:Unavailable";
        public const string AlreadyReserved = "Tables:AlreadyReserved";
    }
    
    public static class Billing
    {
        public const string InsufficientBalance = "Billing:InsufficientBalance";
        public const string InvalidAmount = "Billing:InvalidAmount";
    }
    
    public static class Sessions
    {
        public const string NotFound = "Sessions:NotFound";
        public const string AlreadyEnded = "Sessions:AlreadyEnded";
        public const string InvalidStatus = "Sessions:InvalidStatus";
    }
}

// 使用
return Result.Fail<Guid>(
    "台球桌不可用",
    ErrorCodes.Tables.Unavailable
);
```

**前端处理**:
```typescript
// 前端可以根据错误码做特殊处理
const result = await api.reserveTable(tableId);

if (!result.isSuccess) {
    switch (result.errorCode) {
        case "Tables:Unavailable":
            // 显示"台球桌忙，请选择其他桌台"
            break;
        case "Billing:InsufficientBalance":
            // 显示"余额不足，请充值"并跳转充值页
            break;
        default:
            // 显示通用错误消息
            alert(result.error);
    }
}
```

**日志聚合**:
```csharp
// 日志中包含错误码，便于统计
logger.LogWarning(
    "预订失败: {ErrorCode} - {ErrorMessage}",
    result.ErrorCode,
    result.Error
);

// 可以统计：
// - Tables:Unavailable 出现了多少次
// - Billing:InsufficientBalance 的频率
// - 哪些错误码需要优先优化
```

**迁移策略**:
1. 新代码：强制使用 ErrorCode
2. 老代码：逐步迁移（非强制）
3. 审查时：检查关键路径是否有 ErrorCode

**检查清单**:
- [ ] 业务失败是否返回 ErrorCode？
- [ ] ErrorCode 格式是否符合 `Area:Key`？
- [ ] 是否避免了硬编码字符串？
- [ ] 前端是否能根据 ErrorCode 做差异化处理？

### 8.3 不要"重构洁癖"

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

### 8.4 Handler 行数限制（团队规范）⚠️

> **核心原则**: 行数限制不是为了代码好看，是为了防止业务逻辑失控

> **认知负债真相**: Handler 超行数，本质上是"认知负债"，不是代码问题

**严格的三级行数限制**:

| 行数范围 | 处理策略 | 严重程度 | 认知状态 |
|---------|---------|---------|---------|
| ≤ 40 行 | ✅ 通过审查 | 正常 | 业务语义清晰 |
| 41-60 行 | ⚠️ Code Review 重点检查 | 警告 | **作者已经 hold 不住完整业务语义** |
| 61-80 行 | ❌ 禁止合并（除非有充分理由） | 阻断 | **这个人已经在靠意志力写代码** |
| > 80 行 | 🚨 架构问题，必须重构 | 严重 | 必须强制拆分 |

**现实判断标准**:
- Handler > 60 行 → 作者无法在脑海中维护完整业务流程
- Handler > 80 行 → 编码依靠毅力而非理解
- 这时候再讨论领域服务/Saga/重构已经晚了，**必须强制拆**

**行数计算规则**:
- 只计算 Handler 方法内的有效代码行
- 不包括空行、注释、花括号
- 不包括类定义和方法签名

**40 行以内（标准）**:
```csharp
// ✅ 简洁的 Handler（约 30 行）
public class ReserveTableHandler
{
    public async Task<Result<Guid>> Handle(
        ReserveTable command,
        IDocumentSession session,
        IMessageBus bus,
        CancellationToken ct = default)
    {
        // 1. 加载聚合根（3 行）
        var table = await session
            .LoadAsync<Table>(command.TableId, ct)
            ?? throw new NotFoundException("台球桌不存在");

        // 2. 业务规则校验（3 行）
        if (table.Status != TableStatus.Available)
            return Result.Fail<Guid>(
                "台球桌不可用",
                ErrorCodes.Tables.Unavailable);

        // 3. 执行业务操作（5 行）
        var reservationId = Guid.NewGuid();
        table.Reserve(reservationId, command.MemberId, command.Duration);
        session.Store(table);

        // 4. 发布事件（3 行）
        await bus.PublishAsync(
            new TableReserved(command.TableId, command.MemberId),
            ct);

        return Result.Ok(reservationId);
    }
}
```

**60 行以内（需要 Review）**:
```csharp
// ⚠️ 接近上限，需要 Review 确认复杂度合理
public class ProcessPaymentHandler
{
    public async Task<Result<Guid>> Handle(
        ProcessPayment command,
        IDocumentSession session,
        ILogger<ProcessPaymentHandler> logger)
    {
        // 1. 加载相关数据（10 行）
        var payment = await session.LoadAsync<Payment>(command.PaymentId);
        var member = await session.LoadAsync<Member>(payment.MemberId);
        var session = await session.LoadAsync<Session>(payment.SessionId);
        
        // 2. 多重校验（15 行）
        if (payment == null)
            return Result.Fail<Guid>("支付不存在", ErrorCodes.Payments.NotFound);
        
        if (payment.Status != PaymentStatus.Pending)
            return Result.Fail<Guid>("支付状态错误", ErrorCodes.Payments.InvalidStatus);
        
        if (member.Balance < payment.Amount)
            return Result.Fail<Guid>("余额不足", ErrorCodes.Billing.InsufficientBalance);
        
        // 3. 业务逻辑（15 行）
        member.DeductBalance(payment.Amount);
        payment.Complete();
        session.UpdateBillingStatus(BillingStatus.Paid);
        
        // 4. 持久化和事件（10 行）
        session.Store(member);
        session.Store(payment);
        session.Store(session);
        
        await bus.PublishAsync(new PaymentCompleted(payment.Id));
        
        logger.LogInformation("支付完成 {PaymentId}", payment.Id);
        
        return Result.Ok(payment.Id);
    }
}
```

**超过 60 行（必须重构）**:
```csharp
// ❌ 超过 60 行，必须拆分
public class ProcessOrderHandler  // 假设 80+ 行
{
    public async Task Handle(ProcessOrder cmd)
    {
        // 库存检查（15 行）
        // 价格计算（20 行）
        // 优惠券处理（15 行）
        // 积分计算（15 行）
        // 支付处理（15 行）
        // 总计：80+ 行，太复杂！
    }
}

// ✅ 解决方案 1：拆分成多个 Handler
public class CheckStockHandler { /* 15 行 */ }
public class CalculatePriceHandler { /* 20 行 */ }
public class ApplyCouponHandler { /* 15 行 */ }

// ✅ 解决方案 2：使用 Saga 编排
public class OrderProcessingSaga : Saga
{
    public void Handle(OrderCreated evt) => /* 触发库存检查 */;
    public void Handle(StockReserved evt) => /* 触发价格计算 */;
    public void Handle(PriceCalculated evt) => /* 触发支付 */;
}

// ✅ 解决方案 3：提取领域服务
public class ProcessOrderHandler
{
    public async Task Handle(
        ProcessOrder cmd,
        OrderPricingService pricingService,  // 领域服务
        OrderInventoryService inventoryService)
    {
        // Handler 只负责编排，复杂逻辑在领域服务
        var price = await pricingService.CalculateAsync(cmd);
        var reserved = await inventoryService.ReserveAsync(cmd);
        // 总计：30 行以内
    }
}
```

**警告信号（除了行数）**:
- 包含 5+ 个 if/else 分支
- 需要注入 5+ 个依赖
- 嵌套深度 > 3 层
- 包含复杂的算法逻辑

**Code Review 检查清单**:
- [ ] Handler 行数 ≤ 40 行？
- [ ] 如果 > 40 行，是否有合理理由？
- [ ] 是否可以提取领域服务？
- [ ] 是否可以拆分成多个 Handler？
- [ ] 是否应该使用 Saga？

**自动化检查（可选）**:
```csharp
// 在 CI 中添加行数检查
public class HandlerLineCountAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.RegisterMethodBodyAction(ctx =>
        {
            var method = ctx.MethodSymbol;
            if (method.Name == "Handle")
            {
                var lineCount = CountLines(ctx.MethodBody);
                if (lineCount > 60)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ARCH001",
                            "Handler 超过 60 行",
                            "Handler {0} 有 {1} 行，超过 60 行限制",
                            "Architecture",
                            DiagnosticSeverity.Error),
                        method.Locations[0],
                        method.Name,
                        lineCount));
                }
            }
        });
    }
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

## 十一、何时可以打破这些规则 ⚠️

> **前瞻性说明**: 所有铁律都有一个问题——新手会把规则当信仰，老手需要知道何时叛教

### 11.1 规则不是绝对真理

本文档的规则设计用于：
- 防止架构腐烂
- 降低认知负担
- 保证长期可维护性

但它们**不是为了制造教条**。

### 11.2 可以破例的场景（示例）

**场景 1：小模块（< 5 个 UseCase）是否一定要 Module Marker？**

- ❌ 教条：必须实现，否则不规范
- ✅ 务实：如果模块确实很小且不会扩展，可以暂缓
- ⚠️ 但必须：在文档中说明原因

**场景 2：内部工具模块要不要 Vertical Slice？**

- ❌ 教条：必须垂直切片，否则不符合架构
- ✅ 务实：内部工具（如数据迁移、管理脚本）可以更灵活
- ⚠️ 但必须：与业务模块明确隔离

**场景 3：管理后台是否可以放松行数限制？**

- ❌ 教条：后台也必须 40 行以内
- ✅ 务实：CRUD 密集的后台可以放宽到 60 行
- ⚠️ 但必须：不能放松到 100+ 行（认知崩溃阈值）

**场景 4：原型阶段是否需要严格遵守？**

- ❌ 教条：原型也要完全合规
- ✅ 务实：原型可以快速验证，但需要在正式开发前重构
- ⚠️ 但必须：明确标记为"原型代码"，设定重构 deadline

### 11.3 破例的铁律

**可以破例，但必须**:

1. **写清楚理由**（在代码注释或文档中）
2. **评估影响范围**（只影响局部 vs 影响架构）
3. **设定归还债务的时间**（技术债必须有还款计划）
4. **团队达成共识**（不能个人私自决定）

**示例**:
```csharp
// ⚠️ 架构破例说明
// 原因：此模块为临时数据迁移工具，生命周期 < 1 个月
// 破例内容：不实现 IWolverineModule，不使用垂直切片
// 归还计划：迁移完成后删除此模块
// 批准人：@架构组 2026-01-12
namespace Zss.BilliardHall.Tools.DataMigration;
```

### 11.4 绝对不能破例的红线

以下规则**无论如何都不能破**:

- ❌ 在 BuildingBlocks 中放业务规则
- ❌ 跨服务使用 InvokeAsync
- ❌ 创建 Application/Domain/Infrastructure 分层
- ❌ 创建 Shared Service 跨模块直接调用
- ❌ Integration Event 破坏兼容性

这些是"架构的生命线"，一旦破例，架构会快速腐烂。

### 11.5 平衡原则

**终极判断标准**:

> 破例之后，是否让**三年后的团队**更难维护？

- 如果答案是"是" → 不能破例
- 如果答案是"不会" → 可以评估破例
- 如果答案是"不确定" → 默认不破例

**记住**: 
- 架构规范的目的是避免"全盘重写"
- 不是为了写"优雅代码"
- 也不是为了炫技

---

## 十二、关键要点速查表

### 核心架构原则

✅ **必须遵守**:
- 100% 垂直切片架构，拒绝传统分层
- Handler 即 Application Service，一等公民
- 模块间通过消息通信，禁止共享服务
- BuildingBlocks 严格准入：3 个模块真实使用

❌ **严格禁止**:
- 创建 Application/Domain/Infrastructure 分层
- 创建 Repository/UnitOfWork 接口
- 跨进程使用 InvokeAsync（只能用于进程内）
- 将业务代码放入 BuildingBlocks

### 事件分类边界

| 事件类型 | 范围 | 存放位置 | 可修改性 |
|---------|------|---------|---------|
| Domain Event | 模块内 | `Modules/{Module}/Events/` | ✅ 可自由修改 |
| Module Event | 跨模块 | `Modules/{Module}/Events/` | ⚠️ 需考虑消费者 |
| Integration Event | 跨服务 | `BuildingBlocks/Contracts/` | ❌ 严格版本管理 |

### Saga 使用三条铁律

只有同时满足以下 3 条才使用 Saga：
1. ✅ 跨模块（2 个以上模块）
2. ✅ 跨时间（持续 > 1 分钟）
3. ✅ 需要补偿（不是简单回滚）

### Handler 行数限制

| 行数 | 处理策略 |
|------|---------|
| ≤ 40 | ✅ 通过 |
| 41-60 | ⚠️ Review |
| 61-80 | ❌ 禁止合并 |
| > 80 | 🚨 架构问题 |

### Result<T> 错误码规范

**格式**: `{Area}:{Key}`

**示例**:
- `Tables:NotFound`
- `Billing:InsufficientBalance`
- `Sessions:AlreadyEnded`

**用途**:
- 前端错误识别
- 日志聚合统计
- 重试策略配置

### 跨模块通信规则

| 场景 | 方式 | 工具 |
|------|------|------|
| 模块内 | 直接调用 | 方法调用 |
| 跨模块同步 | Command Bus | `InvokeAsync()` (进程内) |
| 跨模块异步 | Event | `PublishAsync()` |
| 跨服务 | Queue/HTTP | RabbitMQ/Kafka/API |

### Code Review 检查清单

**架构层面**:
- [ ] 是否遵循垂直切片（无分层结构）
- [ ] 是否有共享服务（应拒绝）
- [ ] BuildingBlocks 新增是否满足 3 模块规则
- [ ] 跨服务调用是否避免 InvokeAsync

**事件层面**:
- [ ] 事件是否明确分类（Domain/Module/Integration）
- [ ] Integration Event 是否在 BuildingBlocks/Contracts
- [ ] Module Event 是否有消费者文档

**Handler 层面**:
- [ ] Handler 行数 ≤ 40 行（或有合理理由）
- [ ] 业务失败是否返回 ErrorCode
- [ ] 是否使用 [Transactional] 特性
- [ ] 是否避免手动 SaveChanges

**Saga 层面**:
- [ ] 是否满足 Saga 三条铁律
- [ ] 是否可以用 Handler 或 Event 替代
- [ ] 是否有超时和补偿逻辑

### 模块标记清单

每个模块必须包含：
- [ ] `{Module}Module.cs` 实现 `IWolverineModule`
- [ ] 显式的 `ModuleName` 属性
- [ ] 模块级日志标识

---

## 附录 A：Wolverine 框架核心特性详解

### A.1 基于约定的消息处理

Wolverine 使用**约定优于配置**的理念，自动发现和注册处理器：

```csharp
// 定义命令 - 普通 record 即可
public record CreateMemberCommand(
    string Name,
    string Phone,
    string Email
);

// 定义处理器 - 无需接口或基类
public class CreateMemberHandler
{
    // Wolverine 自动注入依赖到方法参数
    public async Task<MemberCreated> Handle(
        CreateMemberCommand command,
        IDocumentSession session,
        ILogger<CreateMemberHandler> logger,
        CancellationToken ct)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone,
            Email = command.Email,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(member);
        await session.SaveChangesAsync(ct);

        logger.LogInformation("创建会员成功: {MemberId}", member.Id);

        // 返回值自动作为级联消息发布
        return new MemberCreated(member.Id);
    }
}
```

**约定规则**:
- 方法名必须是 `Handle` 或 `HandleAsync`
- 第一个参数是消息/命令/查询
- 其他参数由 Wolverine 自动注入（服务、上下文等）
- 返回值可以是 `void`、`Task`、结果对象或事件

### A.2 消息总线使用

Wolverine 提供了轻量级的消息总线，支持进程内和跨进程通信：

```csharp
public class SomeService
{
    private readonly IMessageBus _bus;

    // 发送命令（同步等待结果）- 进程内
    public async Task<MemberCreated> CreateMember()
    {
        var result = await _bus.InvokeAsync<MemberCreated>(
            new CreateMemberCommand("张三", "13800138000", "zhang@example.com")
        );
        return result;
    }

    // 发布事件（异步，不等待）- 进程内或跨进程
    public async Task NotifyMemberRegistered(Guid memberId)
    {
        await _bus.PublishAsync(new MemberRegisteredEvent(memberId));
    }

    // 发送到特定端点（跨进程）- 外部队列
    public async Task SendPaymentCommand(Guid orderId)
    {
        await _bus.SendAsync(
            new ProcessPaymentCommand(orderId),
            new Uri("rabbitmq://queue/payments")
        );
    }
}
```

**消息类型区别**:
- **Command**: 有且仅有一个处理器，期望得到响应（InvokeAsync）
- **Event**: 可以有多个订阅者，通常无响应（PublishAsync）
- **Query**: 专门用于查询，返回数据（InvokeAsync）

### A.3 中间件和生命周期

Wolverine 支持在消息处理前后执行中间件：

```csharp
// 全局中间件 - 所有消息都会经过
public class LoggingMiddleware
{
    public static async Task Handle(
        IMessageContext context,
        ILogger logger,
        Func<Task> next)
    {
        logger.LogInformation("处理消息: {MessageType}", context.Envelope.MessageType);
        
        var sw = Stopwatch.StartNew();
        await next(); // 调用下一个中间件或 Handler
        sw.Stop();
        
        logger.LogInformation("消息处理完成，耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}

// 特定于 Handler 的特性
[WolverineIgnore]      // 跳过自动发现
[Transactional]        // 自动事务管理
[MaximumAttempts(3)]   // 最大重试次数
public class CreateOrderHandler
{
    public Task Handle(CreateOrderCommand command) { /* ... */ }
}
```

### A.4 HTTP 端点集成

Wolverine 可以将 Handler 直接暴露为 HTTP 端点：

```csharp
// 在 Program.cs 中启用
app.MapWolverineEndpoints();

// 使用特性标记端点
public class GetMemberHandler
{
    [WolverineGet("/api/members/{id}")]
    public async Task<Member?> Handle(
        Guid id,
        IDocumentSession session)
    {
        return await session.LoadAsync<Member>(id);
    }
}

// POST 端点示例
public class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public async Task<IResult> Handle(
        CreateMemberCommand command,
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<Result<Guid>>(command);
        return result.IsSuccess
            ? Results.Created($"/api/members/{result.Value}", result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

### A.5 后台任务和定时任务

```csharp
// 延迟执行
await _bus.ScheduleAsync(
    new SendWelcomeEmailCommand(memberId),
    TimeSpan.FromMinutes(5)
);

// 定时任务（Cron 表达式）
[WolverineHandler]
public static class DailyReportJob
{
    [Schedule("0 0 * * *")] // 每天午夜执行
    public static async Task Execute(
        IDocumentSession session,
        ILogger logger)
    {
        logger.LogInformation("开始生成日报");
        // 生成日报逻辑
    }
}

// 或使用更易读的表达式
[Schedule("daily at 0:00")]
[Schedule("hourly")]
[Schedule("every 5 minutes")]
```

### A.6 持久化 Outbox 模式

Wolverine 与 Marten 集成，提供持久化的消息处理，保证消息不丢失：

```csharp
// 在 Program.cs 中配置
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    // 启用 Wolverine 的 Outbox 集成
    opts.IntegrateWithWolverine();
});

builder.Host.UseWolverine(opts =>
{
    // 使用 Marten 作为消息持久化层
    opts.PersistMessagesWithMarten();
    
    // 配置持久化本地队列
    opts.LocalQueue("important")
        .UseDurableInbox(); // 持久化收件箱
});
```

**Outbox 优势**:
- 消息和数据在同一事务中提交，保证一致性
- 消息持久化到数据库，保证至少被处理一次
- 自动重试失败的消息
- 防止消息丢失

### A.7 错误处理和重试策略

```csharp
// 在 Program.cs 中配置全局重试策略
builder.Host.UseWolverine(opts =>
{
    // 对特定异常进行重试
    opts.Policies.OnException<HttpRequestException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
    // 对特定消息类型配置策略
    opts.Policies.ForMessagesOfType<ProcessPaymentCommand>()
        .MaximumAttempts(5)
        .OnException<PaymentGatewayException>()
        .RetryWithCooldown(1.Seconds(), 5.Seconds(), 10.Seconds());
    
    // 死信队列配置
    opts.Policies.OnException<InvalidOperationException>()
        .MoveToErrorQueue(); // 移到死信队列，不再重试
});

// Handler 中的错误处理
public class ProcessPaymentHandler
{
    public async Task<PaymentResult> Handle(
        ProcessPaymentCommand command,
        IPaymentGateway gateway,
        ILogger logger)
    {
        try
        {
            var result = await gateway.ChargeAsync(command.Amount);
            return PaymentResult.Success(result.TransactionId);
        }
        catch (PaymentGatewayException ex)
        {
            logger.LogError(ex, "支付失败: {OrderId}", command.OrderId);
            
            // Wolverine 会根据配置的策略自动重试
            throw;
        }
    }
}
```

### A.8 监控和诊断

Wolverine 提供了丰富的诊断功能：

```csharp
// 启用详细日志
builder.Logging.AddConsole()
    .SetMinimumLevel(LogLevel.Debug);

// OpenTelemetry 集成
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddWolverineInstrumentation();
        tracing.AddAspNetCoreInstrumentation();
    });

// 健康检查
builder.Services.AddHealthChecks()
    .AddWolverine(); // 检查消息队列健康状态
```

**查看消息状态** (Wolverine + Marten):
```sql
-- 查看收件箱消息
SELECT * FROM wolverine_incoming_messages;

-- 查看发件箱消息
SELECT * FROM wolverine_outgoing_messages;

-- 查看死信（失败的消息）
SELECT * FROM wolverine_dead_letters;
```

---

## 附录 B：Wolverine 与 MediatR 深度对比

### B.1 特性对比表

| 特性 | Wolverine | MediatR | 说明 |
|------|-----------|---------|------|
| **配置方式** | 约定优于配置 | 显式注册 | Wolverine 自动发现，MediatR 需手动注册 |
| **依赖注入** | 方法参数注入 | 构造函数注入 | Wolverine 更灵活，MediatR 更传统 |
| **Handler 接口** | 无需接口 | 需实现 IRequestHandler | Wolverine 更简洁 |
| **中间件** | 内置，基于约定 | Pipeline Behaviors | Wolverine 内置更强大 |
| **消息传输** | 进程内 + 跨进程 | 仅进程内 | Wolverine 支持队列、HTTP 等 |
| **持久化** | Outbox 与 Marten 集成 | 需自行实现 | Wolverine 开箱即用 |
| **事务管理** | `[Transactional]` 特性 | 需手动管理 | Wolverine 自动化 |
| **重试策略** | 内置可配置 | 需自行实现 | Wolverine 强大的重试机制 |
| **后台任务** | 内置调度器 | 需集成 Hangfire 等 | Wolverine 原生支持 |
| **HTTP 集成** | `[WolverineGet/Post]` | 需手动映射 | Wolverine 更便捷 |
| **性能** | 高（编译时代码生成） | 中等（反射） | Wolverine 性能更优 |
| **学习曲线** | 中等（约定较多） | 较低（模式简单） | MediatR 更容易上手 |
| **社区支持** | 中等 | 非常活跃 | MediatR 更成熟 |
| **适用场景** | 复杂业务 + 消息驱动 | 简单 CQRS | 根据需求选择 |

### B.2 代码对比示例

**场景：创建订单并发送通知**

#### MediatR 实现

```csharp
// 1. 定义命令
public class CreateOrderCommand : IRequest<Guid>
{
    public string ProductName { get; set; }
    public decimal Amount { get; set; }
}

// 2. 定义处理器（需要接口）
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly DbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateOrderHandler> _logger;

    // 构造函数注入所有依赖
    public CreateOrderHandler(
        DbContext context,
        IMediator mediator,
        ILogger<CreateOrderHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductName = request.ProductName,
            Amount = request.Amount
        };

        _context.Orders.Add(order);
        
        // 手动管理事务
        await _context.SaveChangesAsync(cancellationToken);

        // 手动发送通知
        await _mediator.Publish(
            new OrderCreatedNotification(order.Id),
            cancellationToken
        );

        return order.Id;
    }
}

// 3. 定义通知
public class OrderCreatedNotification : INotification
{
    public Guid OrderId { get; }
    public OrderCreatedNotification(Guid orderId) => OrderId = orderId;
}

// 4. 定义通知处理器
public class OrderCreatedNotificationHandler 
    : INotificationHandler<OrderCreatedNotification>
{
    private readonly IEmailService _emailService;

    public OrderCreatedNotificationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        await _emailService.SendOrderConfirmationAsync(notification.OrderId);
    }
}

// 5. 注册服务
services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

#### Wolverine 实现

```csharp
// 1. 定义命令（普通 record）
public record CreateOrderCommand(string ProductName, decimal Amount);

// 2. 定义处理器（无需接口，自动事务）
public class CreateOrderHandler
{
    // 方法参数注入，无需构造函数
    [Transactional] // 自动事务管理
    public async Task<OrderCreated> Handle(
        CreateOrderCommand command,
        IDocumentSession session, // Wolverine 自动注入
        ILogger logger)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductName = command.ProductName,
            Amount = command.Amount
        };

        session.Store(order);
        // 无需手动 SaveChanges，[Transactional] 自动处理

        logger.LogInformation("订单创建成功: {OrderId}", order.Id);

        // 返回值自动作为级联消息发布（Outbox 模式）
        return new OrderCreated(order.Id);
    }
}

// 3. 定义事件（普通 record）
public record OrderCreated(Guid OrderId);

// 4. 定义事件处理器（无需接口，自动并发）
public class OrderCreatedHandler
{
    public async Task Handle(
        OrderCreated evt,
        IEmailService emailService)
    {
        await emailService.SendOrderConfirmationAsync(evt.OrderId);
    }
}

// 5. 配置服务（自动发现）
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    opts.PersistMessagesWithMarten(); // Outbox 模式
});
```

**对比总结**:
- **Wolverine**: 5 个文件，无接口，自动事务，级联消息，Outbox 保证一致性
- **MediatR**: 5 个文件，需接口，手动事务，手动发布，无持久化保证

### B.3 选择建议

**选择 Wolverine 的场景**:
- ✅ 需要消息持久化（Outbox 模式）
- ✅ 需要跨进程通信（RabbitMQ、Kafka）
- ✅ 需要后台任务和定时任务
- ✅ 追求高性能（编译时代码生成）
- ✅ 希望减少样板代码（无需接口）
- ✅ 使用 Marten 作为数据访问层

**选择 MediatR 的场景**:
- ✅ 只需要简单的 CQRS 模式
- ✅ 团队已熟悉 MediatR
- ✅ 项目规模较小，不需要复杂功能
- ✅ 希望有更成熟的社区支持
- ✅ 使用 EF Core（Wolverine 更适合 Marten）

**本项目选择 Wolverine 的原因**:
1. 需要 Outbox 模式保证消息和数据一致性
2. 与 Marten 紧密集成，简化开发
3. 垂直切片架构与 Wolverine 约定机制完美契合
4. 需要跨模块异步通信和后台任务
5. 追求更高的开发效率和代码简洁性

---

## 附录 C：测试支持与最佳实践

### C.1 单元测试

```csharp
public class CreateMemberHandlerTests
{
    [Fact]
    public async Task Should_Create_Member_Successfully()
    {
        // Arrange
        var store = DocumentStore.For(opts =>
        {
            opts.Connection(Servers.PostgresConnectionString);
            opts.DatabaseSchemaName = "test";
        });

        await using var session = store.LightweightSession();
        var handler = new CreateMemberHandler();
        var command = new CreateMemberCommand("张三", "13800138000", "zhang@test.com");

        // Act
        var result = await handler.Handle(command, session, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var member = await session.LoadAsync<Member>(result.MemberId);
        member.Should().NotBeNull();
        member.Name.Should().Be("张三");
    }
}
```

### C.2 集成测试

使用 Wolverine 提供的测试工具：

```csharp
public class OrderWorkflowTests : IAsyncLifetime
{
    private IAlbaHost _host;

    public async Task InitializeAsync()
    {
        // 启动测试主机
        _host = await AlbaHost.For<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 配置测试数据库
                services.AddMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.DatabaseSchemaName = $"test_{Guid.NewGuid():N}";
                });
            });
        });
    }

    [Fact]
    public async Task Should_Complete_Order_Workflow()
    {
        // 1. 发送命令并等待结果
        var orderId = await _host.InvokeMessageAndWaitAsync<Guid>(
            new CreateOrderCommand("Product A", 100m)
        );

        // 2. 验证订单创建
        var session = _host.Services.GetRequiredService<IDocumentSession>();
        var order = await session.LoadAsync<Order>(orderId);
        order.Should().NotBeNull();

        // 3. 验证事件发布（等待事件处理完成）
        await _host.WaitForMessageToBeReceivedAsync<OrderCreated>();

        // 4. 验证副作用（如邮件发送）
        var emailService = _host.Services.GetRequiredService<IEmailService>();
        // 验证邮件已发送...
    }

    public async Task DisposeAsync()
    {
        await _host.DisposeAsync();
    }
}
```

### C.3 测试最佳实践

**AAA 模式** (Arrange-Act-Assert):
```csharp
[Fact]
public async Task Should_Fail_When_Table_Not_Available()
{
    // Arrange - 准备测试数据
    var table = new Table { Id = tableId, Status = TableStatus.Occupied };
    session.Store(table);
    await session.SaveChangesAsync();
    
    var command = new StartSessionCommand(tableId, memberId);

    // Act - 执行操作
    var result = await handler.Handle(command, session);

    // Assert - 验证结果
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Contain("不可用");
}
```

**测试隔离**:
```csharp
// 每个测试使用独立的数据库 schema
public class TestBase : IAsyncLifetime
{
    protected IDocumentStore Store { get; private set; }
    private string _schemaName;

    public async Task InitializeAsync()
    {
        _schemaName = $"test_{Guid.NewGuid():N}";
        Store = DocumentStore.For(opts =>
        {
            opts.Connection(TestConfig.ConnectionString);
            opts.DatabaseSchemaName = _schemaName;
            opts.CreateDatabasesForTenants(c => c.ForTenant());
        });
        
        await Store.Advanced.Clean.CompletelyRemoveAllAsync();
    }

    public async Task DisposeAsync()
    {
        await Store.Advanced.Clean.DeleteAllDocumentsAsync();
        Store?.Dispose();
    }
}
```

---

## 附录 D：从 ABP 分层架构迁移指南

### D.1 迁移步骤

**第一步：识别现有功能**
```
列出所有 Use Case:
- CreateUser
- UpdateUserProfile
- StartSession
- EndSession
- ProcessPayment
- ...
```

**第二步：创建切片目录**
```
为每个 Use Case 创建独立目录:
Modules/
  Members/
    RegisterMember/
    UpdateMemberProfile/
    TopUpBalance/
  Tables/
    ReserveTable/
    ReleaseTable/
```

**第三步：迁移代码**

**迁移前（ABP 分层）**:
```csharp
// HttpApi/TableController.cs
public class TableController : AbpController
{
    private readonly ITableAppService _tableService;
    
    [HttpPost]
    public async Task<TableSessionDto> StartSession(StartSessionDto dto)
    {
        return await _tableService.StartSessionAsync(dto);
    }
}

// Application/TableAppService.cs
public class TableAppService : ApplicationService
{
    private readonly ITableSessionRepository _sessionRepo;
    private readonly ITableRepository _tableRepo;
    
    public async Task<TableSessionDto> StartSessionAsync(StartSessionDto dto)
    {
        var table = await _tableRepo.GetAsync(dto.TableId);
        var session = new TableSession(/* ... */);
        await _sessionRepo.InsertAsync(session);
        return ObjectMapper.Map<TableSessionDto>(session);
    }
}

// Domain/ITableSessionRepository.cs
public interface ITableSessionRepository : IRepository<TableSession, Guid>
{
    // ...
}
```

**迁移后（Wolverine + Vertical Slice）**:
```csharp
// Modules/Tables/StartSession/StartSessionCommand.cs
public record StartSessionCommand(Guid TableId, Guid? MemberId);

// Modules/Tables/StartSession/StartSessionHandler.cs
public class StartSessionHandler
{
    [Transactional]
    public async Task<Result<Guid>> Handle(
        StartSessionCommand command,
        IDocumentSession session)
    {
        var table = await session.LoadAsync<Table>(command.TableId);
        if (table?.Status != TableStatus.Available)
            return Result.Fail<Guid>("台球桌不可用");

        var tableSession = TableSession.Start(command.TableId, command.MemberId);
        session.Store(tableSession);
        
        return Result.Ok(tableSession.Id);
    }
}

// Modules/Tables/StartSession/StartSessionEndpoint.cs
public class StartSessionEndpoint
{
    [WolverinePost("/api/tables/sessions")]
    public async Task<IResult> Handle(
        StartSessionCommand command,
        IMessageBus bus)
    {
        var result = await bus.InvokeAsync<Result<Guid>>(command);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

**第四步：提取共享逻辑**

只提取**真正需要共享**的逻辑：
```csharp
// BuildingBlocks/Domain/IPricingService.cs
public interface IPricingService
{
    decimal CalculatePrice(TimeSpan duration, TableType type);
}

// 在多个 Handler 中使用
public class CalculateBillingHandler
{
    public async Task Handle(
        CalculateBillingCommand command,
        IPricingService pricingService)
    {
        var price = pricingService.CalculatePrice(
            command.Duration,
            command.TableType
        );
        // ...
    }
}
```

**第五步：重构通信方式**

**迁移前（直接调用）**:
```csharp
public class EndSessionHandler
{
    private readonly IBillingService _billingService;
    
    public async Task Handle(EndSessionCommand cmd)
    {
        // 直接调用其他服务
        await _billingService.CalculateBillingAsync(cmd.SessionId);
    }
}
```

**迁移后（消息通信）**:
```csharp
public class EndSessionHandler
{
    public async Task<SessionEnded> Handle(
        EndSessionCommand cmd,
        IDocumentSession session)
    {
        // 完成会话
        var tableSession = await session.LoadAsync<TableSession>(cmd.SessionId);
        tableSession.End(DateTime.UtcNow);
        
        // 返回事件，Wolverine 自动发布
        return new SessionEnded(cmd.SessionId, tableSession.Duration);
    }
}

// 计费模块监听事件
public class SessionEndedHandler
{
    public async Task Handle(SessionEnded evt, IDocumentSession session)
    {
        // 自动触发计费
        var billing = Billing.Calculate(evt.SessionId, evt.Duration);
        session.Store(billing);
    }
}
```

### D.2 迁移检查清单

- [ ] 所有 Controller → Endpoint 已迁移
- [ ] 所有 AppService → Handler 已迁移
- [ ] Repository 已移除（使用 IDocumentSession）
- [ ] DTO Mapping 已移除（直接使用 Command/Query）
- [ ] 跨层调用已改为消息通信
- [ ] 单元测试已更新
- [ ] 集成测试已更新
- [ ] 文档已更新

---

## 附录 E：命名约定与代码组织

### E.1 命名规范

**消息命名**:
```csharp
// Command: 动词 + 名词 + Command
CreateMemberCommand
UpdateTableStatusCommand
ProcessPaymentCommand
CancelOrderCommand

// Event: 名词 + 动词过去式 + Event（或直接过去式）
MemberCreatedEvent  // 或 MemberCreated
SessionStartedEvent // 或 SessionStarted
PaymentProcessedEvent
OrderCancelled

// Query: Get/Find/Search + 名词 + Query
GetMemberByIdQuery
SearchTablesQuery
FindActiveSessionsQuery
ListRecentOrdersQuery
```

**Handler 命名**:
```csharp
// 与消息同名 + Handler
CreateMemberHandler
UpdateTableStatusHandler
MemberCreatedEventHandler  // 或 MemberCreatedHandler
GetMemberByIdHandler
```

**文件夹命名**:
```csharp
// 与 Use Case 对应，使用 PascalCase
RegisterMember/
TopUpBalance/
StartSession/
ProcessPayment/
```

### E.2 文件组织模板

**标准切片结构**:
```
Modules/
  Members/
    RegisterMember/
      RegisterMember.cs              # Command 定义
      RegisterMemberHandler.cs       # Handler
      RegisterMemberValidator.cs     # FluentValidation（可选）
      RegisterMemberEndpoint.cs      # HTTP 端点（可选）
      MemberRegistered.cs            # 领域事件
    Member.cs                        # 聚合根
    MemberTier.cs                    # 枚举/值对象
    MembersModule.cs                 # 模块标记
```

**最小化切片**:
```
Modules/
  Members/
    GetMember/
      GetMember.cs                   # Query
      GetMemberHandler.cs            # Handler
```

### E.3 代码风格示例

```csharp
// ✅ 推荐：简洁的 record 定义
public record CreateMemberCommand(
    string Name,
    string Phone,
    string Email
);

// ✅ 推荐：方法参数注入
public class CreateMemberHandler
{
    [Transactional]
    public async Task<MemberCreated> Handle(
        CreateMemberCommand command,
        IDocumentSession session,
        ILogger<CreateMemberHandler> logger,
        CancellationToken ct = default)
    {
        // 业务逻辑
    }
}

// ❌ 避免：构造函数注入（Wolverine 不推荐）
public class CreateMemberHandler
{
    private readonly IDocumentSession _session;
    private readonly ILogger _logger;
    
    public CreateMemberHandler(IDocumentSession session, ILogger logger)
    {
        _session = session;
        _logger = logger;
    }
}

// ❌ 避免：实现接口（不需要）
public class CreateMemberHandler : IRequestHandler<CreateMemberCommand>
{
    // ...
}
```

---

## 十三、参考资源

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

### 项目内部文档
- `doc/06_开发规范/Saga使用指南.md` - Saga 详细使用指南
- `doc/06_开发规范/FluentValidation集成指南.md` - 输入验证最佳实践
- `doc/06_开发规范/代码风格.md` - 代码风格规范
- `doc/06_开发规范/日志规范.md` - 日志记录规范

---

## 十四、版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0.0 | 2024-01-15 | 初始版本，完整蓝图 |
| 1.1.0 | 2026-01-12 | **重大强化**：添加 4 大隐藏风险缓解措施和 3 大架构升级建议<br/>- ⚠️ 事件分类边界管理（Domain/Module/Integration）<br/>- ⚠️ 收紧 Saga 使用标准（3 条铁律）<br/>- ⚠️ Result<T> 错误码支持（防止错误模型失控）<br/>- ⚠️ BuildingBlocks 防污染铁律（3 模块规则）<br/>- 🔧 显式 Module Marker 设计<br/>- 🔧 禁止跨进程同步命令（InvokeAsync 限制）<br/>- 🔧 Handler 行数限制团队规范（40/60/80）<br/>- 📝 关键要点速查表<br/>- 📝 Code Review 检查清单 |
| 1.2.0 | 2026-01-12 | **架构师反馈强化**：基于资深架构师深度评审，加强防护栏<br/>- 🛡️ BuildingBlocks 第 5 条隐含规则（抽象后修改成本必须降低）<br/>- 🛡️ ErrorCodes 高级陷阱警告（禁止承载业务语义）<br/>- 🛡️ Module Event 显式声明要求（PublicEvents 文件夹或注释标记）<br/>- 🛡️ Integration Event 不可修改铁律强化（只增不改，包含演进示例）<br/>- 💡 Saga 心理刹车（犹豫时默认不用）<br/>- 💡 Handler 认知负债说明（> 60 行 = 认知崩溃）<br/>- 📖 新增第十一章：何时可以打破这些规则<br/>- 📖 破例铁律、红线清单、平衡原则 |
| 1.3.0 | 2026-01-13 | **文档归并强化**：整合垂直切片和框架介绍文档，建立单一真相源<br/>- 📚 新增序章：垂直切片架构理念（整合自《垂直切片架构说明》）<br/>- 📚 新增附录 A：Wolverine 框架核心特性详解（整合自《Wolverine 框架介绍》）<br/>- 📚 新增附录 B：Wolverine 与 MediatR 深度对比<br/>- 📚 新增附录 C：测试支持与最佳实践<br/>- 📚 新增附录 D：从 ABP 分层架构迁移指南<br/>- 📚 新增附录 E：命名约定与代码组织<br/>- ✅ 完成文档瘦身建议 #5（P1 优先级）：归并 3 个 Wolverine 相关文档为 1 个<br/>- ✅ 建立架构知识的单一真相源（SSOT）<br/>- ✅ 保持内容完整性，无信息丢失 |

---

**最后更新**: 2026-01-13  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核  
**文档归并**: ✅ 已完成（归并《垂直切片架构说明》和《Wolverine 框架介绍》）  
**社区反馈**: 已整合资深架构师深度评审意见
