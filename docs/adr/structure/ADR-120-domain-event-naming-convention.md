# ADR-120：领域事件命名规范

**状态**：✅ 已采纳（Adopted）  
**级别**：结构约束（Structure Constraint）  
**适用范围**：所有模块（Modules）、领域事件定义、事件处理器  
**生效时间**：即刻  
**依赖 ADR**：ADR-0001（模块化单体与垂直切片架构）、ADR-0003（命名空间规范）、ADR-0005（应用内交互模型）

---

## 聚焦内容（Focus）

- 统一领域事件命名规则，提升跨模块协作识别度
- 规范事件类型前缀、后缀和命名空间组织
- 支持事件版本演进和兼容性管理
- 明确命名空间、文件名、类型名三者同步规则
- 为架构测试、文档生成和工具链自动发现提供标准基础
- 严格遵守模块隔离原则，避免事件命名嵌入跨模块业务语义

---

## 术语表（Glossary）

| 术语                      | 定义                               |
|-------------------------|----------------------------------|
| 领域事件（Domain Event）      | 描述模块内已发生业务事实的不可变对象，用于模块内或跨模块异步通信 |
| 集成事件（Integration Event） | 跨模块、跨系统的事件，通常由领域事件转换而来，用于外部集成    |
| 事件名称                    | 事件类型的完整名称，遵循动词过去式+名词模式           |
| 事件命名空间                  | 事件类型所在的命名空间，必须与物理文件结构对应          |
| 事件版本                    | 事件结构的版本标识，用于支持向后兼容和演进            |
| 事件聚合根                   | 事件所属的聚合根实体，决定事件的业务语义边界           |

---

## 决策（Decision）

### 基本命名规则

#### 事件类型命名模式

所有领域事件必须遵循以下命名模式：

```
{AggregateRoot}{Action}Event[{Version}]
```

**组成部分说明**：

- **{AggregateRoot}**：聚合根名称（单数形式，PascalCase）
- **{Action}**：动词过去式（PascalCase），描述已发生的业务动作
- **Event**：固定后缀（必须）
- **{Version}**：可选版本标识（如 V2、V3），用于演进场景

**✅ 正确示例**：

```csharp
// 模块：Orders
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);
public record OrderPaidEvent(Guid OrderId, decimal Amount, DateTime PaidAt);
public record OrderCancelledEvent(Guid OrderId, string Reason);
public record OrderShippedEvent(Guid OrderId, string TrackingNumber);

// 模块：Members
public record MemberRegisteredEvent(Guid MemberId, string Username, DateTime RegisteredAt);
public record MemberUpgradedEvent(Guid MemberId, string NewLevel, DateTime UpgradedAt);
public record MemberSuspendedEvent(Guid MemberId, string Reason, DateTime SuspendedAt);

// 带版本的演进示例
public record OrderCreatedEventV2(Guid OrderId, Guid MemberId, DateTime CreatedAt, string Source);
```

**❌ 错误示例**：

```csharp
// ❌ 缺少 Event 后缀
public record OrderCreated(Guid OrderId);

// ❌ 使用现在时或进行时
public record OrderCreating(Guid OrderId);
public record OrderCreate(Guid OrderId);

// ❌ 使用动词原形
public record OrderCreateEvent(Guid OrderId);

// ❌ 嵌入跨模块业务语义
public record OrderCreatedAndMemberPointsAddedEvent(Guid OrderId, Guid MemberId, int Points);

// ❌ 使用模糊或非业务语义的名称
public record OrderEvent(Guid OrderId, string Action);
public record OrderDataChangedEvent(Guid OrderId);
```

#### 事件属性命名

- 事件应使用 `record` 定义（不可变对象）
- 主键属性命名：`{AggregateRoot}Id`（如 `OrderId`、`MemberId`）
- 时间戳属性：使用 `OccurredAt` 或特定动作时间（如 `CreatedAt`、`PaidAt`）
- 避免使用通用属性名（如 `Id`、`Time`），明确业务语义

**✅ 正确示例**：

```csharp
public record OrderCreatedEvent(
    Guid OrderId,           // 明确的聚合根 ID
    Guid MemberId,          // 关联实体 ID
    DateTime CreatedAt,     // 明确的时间语义
    int ItemCount           // 业务数据
);
```

**❌ 错误示例**：

```csharp
public record OrderCreatedEvent(
    Guid Id,                // ❌ 不明确
    DateTime Time,          // ❌ 语义模糊
    object Data             // ❌ 类型不明确
);
```

### 命名空间组织规则

#### 命名空间映射

领域事件必须组织在模块的 `Events` 命名空间下，严格遵循 ADR-0003 命名空间规范：

```
Zss.BilliardHall.Modules.{ModuleName}.Events[.{SubNamespace}]
```

**✅ 正确命名空间示例**：

```csharp
namespace Zss.BilliardHall.Modules.Orders.Events;

public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);
```

```csharp
namespace Zss.BilliardHall.Modules.Members.Events;

public record MemberRegisteredEvent(Guid MemberId, string Username, DateTime RegisteredAt);
```

**子命名空间组织（可选）**：

对于复杂模块，可使用子命名空间按聚合根或业务领域分组：

```csharp
namespace Zss.BilliardHall.Modules.Orders.Events.OrderLifecycle;

public record OrderCreatedEvent(...);
public record OrderCompletedEvent(...);
```

```csharp
namespace Zss.BilliardHall.Modules.Orders.Events.Payment;

public record OrderPaidEvent(...);
public record OrderRefundedEvent(...);
```

**❌ 禁止的命名空间模式**：

```csharp
// ❌ 不在模块内
namespace Zss.BilliardHall.Events;

// ❌ 使用 Domain 或其他非标准命名空间
namespace Zss.BilliardHall.Modules.Orders.Domain.Events;

// ❌ 使用 Shared/Common 等命名空间
namespace Zss.BilliardHall.Shared.Events;
```

#### 文件结构组织

领域事件的物理文件结构必须与命名空间严格对应：

```
src/Modules/{ModuleName}/
  ├── Events/
  │    ├── OrderCreatedEvent.cs
  │    ├── OrderPaidEvent.cs
  │    ├── OrderCancelledEvent.cs
  │    └── OrderShippedEvent.cs
  └── EventHandlers/
       ├── OrderCreatedEventHandler.cs
       └── MemberUpgradedEventHandler.cs  (订阅其他模块事件)
```

**文件命名规则**：

- 每个事件独立文件
- 文件名 = 类型名（如 `OrderCreatedEvent.cs`）
- 禁止一个文件包含多个事件定义（除非是同一事件的多个版本）

**✅ 正确文件组织**：

```
src/Modules/Orders/Events/OrderCreatedEvent.cs
src/Modules/Orders/Events/OrderPaidEvent.cs
src/Modules/Orders/EventHandlers/OrderCreatedEventHandler.cs
```

**❌ 错误文件组织**：

```
src/Modules/Orders/Domain/Events/OrderCreatedEvent.cs  // ❌ 路径不符合规范
src/Modules/Orders/OrderEvents.cs                      // ❌ 多个事件在一个文件
src/Shared/Events/OrderCreatedEvent.cs                 // ❌ 放在共享目录
```

### 事件处理器命名规则

事件处理器（EventHandler）必须遵循以下命名规则：

#### 基础命名模式

```
{EventName}Handler
```

**✅ 正确示例**：

```csharp
// 处理本模块事件
namespace Zss.BilliardHall.Modules.Orders.EventHandlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent @event)
    {
        // 处理逻辑
    }
}

// 订阅其他模块事件
namespace Zss.BilliardHall.Modules.Members.EventHandlers;

public class OrderPaidEventHandler : IEventHandler<OrderPaidEvent>  // 订阅 Orders 模块事件
{
    public async Task Handle(OrderPaidEvent @event)
    {
        // 更新会员积分
    }
}
```

#### 扩展命名模式（多订阅场景）

当同一个事件被多个模块订阅时，为了在日志、APM、追踪和文档中更好地区分不同的处理目的，**允许**使用扩展命名模式：

```
{EventName}{Purpose}Handler
```

**✅ 扩展模式示例**（同一事件的多个订阅者）：

```csharp
// Members 模块：处理积分
namespace Zss.BilliardHall.Modules.Members.EventHandlers;

public class OrderPaidEventAddPointsHandler : IEventHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent @event)
    {
        // 为会员增加积分
    }
}

// Accounting 模块：生成发票
namespace Zss.BilliardHall.Modules.Accounting.EventHandlers;

public class OrderPaidEventGenerateInvoiceHandler : IEventHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent @event)
    {
        // 生成财务发票
    }
}

// Notifications 模块：发送通知
namespace Zss.BilliardHall.Modules.Notifications.EventHandlers;

public class OrderPaidEventSendNotificationHandler : IEventHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent @event)
    {
        // 发送支付成功通知
    }
}
```

**命名建议**：

- **Purpose** 部分应清晰描述处理器的业务意图（如 `AddPoints`、`GenerateInvoice`、`SendNotification`）
- 对于简单场景（单一订阅者），使用基础模式 `{EventName}Handler` 即可
- 对于复杂场景（多个订阅者），使用扩展模式以提高可观测性

**❌ 错误示例**：

```csharp
// ❌ 缺少 Handler 后缀
public class OrderCreatedProcessor : IEventHandler<OrderCreatedEvent> { }

// ❌ 使用 Service 后缀
public class OrderCreatedService : IEventHandler<OrderCreatedEvent> { }

// ❌ 命名不对应事件名称
public class OrderEventHandler : IEventHandler<OrderCreatedEvent> { }

// ❌ Purpose 部分含糊不清
public class OrderPaidEventHandler1 : IEventHandler<OrderPaidEvent> { }
public class OrderPaidEventHandler2 : IEventHandler<OrderPaidEvent> { }
```

### 集成事件命名规则

集成事件用于跨系统通信，通常从领域事件转换而来，命名遵循类似规则：

```
{AggregateRoot}{Action}IntegrationEvent[{Version}]
```

**✅ 正确示例**：

```csharp
namespace Zss.BilliardHall.Integration.Events;

// 对外发布的集成事件
public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid MemberId,
    DateTime CreatedAt,
    string ExternalId  // 包含外部系统需要的额外信息
);
```

**转换示例**：

```csharp
// 从领域事件转换为集成事件
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;

    public async Task Handle(OrderCreatedEvent @event)
    {
        // 转换并发布集成事件
        var integrationEvent = new OrderCreatedIntegrationEvent(
            @event.OrderId,
            @event.MemberId,
            @event.CreatedAt,
            GenerateExternalId(@event.OrderId)
        );

        await _integrationEventBus.PublishAsync(integrationEvent);
    }
}
```

### 事件版本演进规则

当事件结构需要演进时，遵循以下规则：

#### 版本标识

使用 `V{N}` 后缀标识版本号（N 从 2 开始）：

```csharp
// V1（初始版本，不带版本后缀）
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);

// V2（新增字段）
public record OrderCreatedEventV2(
    Guid OrderId, 
    Guid MemberId, 
    DateTime CreatedAt,
    string Source,        // 新增：订单来源
    string Channel        // 新增：订单渠道
);
```

#### 版本兼容性

- **向后兼容**：新版本必须能够从旧版本转换
- **同时支持**：可以在过渡期同时发布两个版本的事件
- **废弃策略**：明确旧版本的废弃时间和迁移路径

**⚠️ 重要说明**：

> **事件类型版本命名（如 `V2`）≠ 序列化兼容性策略**
>
> 本 ADR 定义的版本命名仅限于**代码层面的类型标识**。实际的序列化兼容性、Schema 版本管理、消费者版本声明等跨进程/跨系统的兼容性策略，将在技术层
> ADR（ADR-300 系列：Integration / Messaging）中定义。
>
> 不要误以为添加 `V2` 后缀就能自动实现跨系统的兼容性保障。

**✅ 版本转换示例**：

```csharp
// V1 到 V2 的转换适配器
public class OrderCreatedEventAdapter
{
    public static OrderCreatedEventV2 ToV2(OrderCreatedEvent v1Event)
    {
        return new OrderCreatedEventV2(
            v1Event.OrderId,
            v1Event.MemberId,
            v1Event.CreatedAt,
            Source: "Unknown",    // 默认值
            Channel: "Web"        // 默认值
        );
    }
}
```

### 模块隔离约束

领域事件必须严格遵守 ADR-0001 的模块隔离原则：

#### ✅ 允许的模式

1. **模块内事件发布和订阅**：

```csharp
// Orders 模块内部
await _eventBus.PublishAsync(new OrderCreatedEvent(orderId, memberId, DateTime.UtcNow));
```

2. **跨模块异步事件订阅**：

```csharp
// Members 模块订阅 Orders 模块事件
public class OrderPaidEventHandler : IEventHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent @event)
    {
        // 更新会员积分（通过 MemberId，不依赖 Order 实体）
        await _memberRepository.AddPointsAsync(@event.MemberId, CalculatePoints(@event.Amount));
    }
}
```

3. **事件仅包含原始类型和 DTO**：

```csharp
// ✅ 正确：只包含原始类型和简单 DTO
public record OrderCreatedEvent(
    Guid OrderId,
    Guid MemberId,
    DateTime CreatedAt,
    List<OrderItemDto> Items  // DTO，不是领域实体
);

public record OrderItemDto(string ProductId, int Quantity, decimal Price);
```

#### ❌ 禁止的模式

1. **事件包含领域实体**：

```csharp
// ❌ 禁止：事件包含领域实体
public record OrderCreatedEvent(
    Order Order,          // ❌ 领域实体
    Member Member         // ❌ 领域实体
);
```

2. **事件嵌入跨模块业务逻辑**：

```csharp
// ❌ 禁止：事件名称包含多模块业务语义
public record OrderCreatedAndPointsAddedEvent(Guid OrderId, Guid MemberId, int Points);

// ✅ 正确：分为两个独立事件
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);
public record MemberPointsAddedEvent(Guid MemberId, int Points, string Reason);
```

3. **事件包含业务判断方法**：

```csharp
// ❌ 禁止：事件包含业务方法
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt)
{
    public bool CanBeCancelled() => DateTime.UtcNow - CreatedAt < TimeSpan.FromHours(24); // ❌
}

// ✅ 正确：事件只包含数据，判断逻辑在领域模型或处理器中
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);
```

---

## 快速参考表（Quick Reference）

| 约束编号      | 描述                      | 层级     | 测试用例/自动化                                      | 章节        |
|-----------|-------------------------|--------|-----------------------------------------------|-----------|
| 约束编号      | 描述                      | 层级     | 测试用例/自动化                                      | 章节        |
| --------- | ------                  | ------ | ----------------                              | ------    |
| ADR-120.1 | 事件类型必须以 `Event` 后缀结尾    | L1     | Event_Types_Should_End_With_Event_Suffix      | 基本命名规则    |
| ADR-120.2 | 事件名称必须使用动词过去式           | L1     | Event_Names_Should_Use_Past_Tense_Verbs       | 基本命名规则    |
| ADR-120.3 | 事件必须在模块的 `Events` 命名空间下 | L1     | Events_Should_Be_In_Events_Namespace          | 命名空间组织规则  |
| ADR-120.4 | 事件处理器必须以 `Handler` 后缀结尾 | L1     | Event_Handlers_Should_End_With_Handler_Suffix | 事件处理器命名规则 |
| ADR-120.5 | 事件不得包含领域实体类型            | L1     | Events_Should_Not_Contain_Domain_Entities     | 模块隔离约束    |
| ADR-120.6 | 事件不得包含业务方法              | L1     | Events_Should_Not_Contain_Business_Methods    | 模块隔离约束    |
| ADR-120.7 | 事件文件名必须与类型名一致           | L2     | 人工 Code Review                                | 文件结构组织    |
| ADR-120.8 | 事件版本标识使用 `V{N}` 格式      | L2     | Event_Versions_Should_Use_VN_Format           | 事件版本演进规则  |

**测试层级说明**：

- **L1**：必须架构测试覆盖，CI 自动阻断（本体语义约束，违反即为架构退化）
- **L2**：建议架构测试覆盖或人工 Code Review（技术实践约束，可根据情况灵活处理）

**注**：ADR-120.7 从 L1 降级为 L2，因其耦合仓库物理结构（monorepo、目录重组等），属于代码组织习惯而非架构本体约束。

---

## 架构测试实施建议

在 `src/tests/ArchitectureTests/ADR/ADR_0120_Architecture_Tests.cs` 中实现以下测试：

```csharp
[Fact]
public void Event_Types_Should_End_With_Event_Suffix()
{
    var result = Types.InAssemblies(ModuleAssemblies)
        .That()
        .ResideInNamespace("*.Events")
        .Should()
        .HaveNameEndingWith("Event")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        "所有事件类型必须以 'Event' 后缀结尾 (ADR-120.1)");
}

[Fact]
public void Events_Should_Be_In_Events_Namespace()
{
    var result = Types.InAssemblies(ModuleAssemblies)
        .That()
        .HaveNameEndingWith("Event")
        .Should()
        .ResideInNamespace("*.Modules.*.Events")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        "所有事件必须在模块的 Events 命名空间下 (ADR-120.3)");
}

[Fact]
public void Events_Should_Not_Contain_Domain_Entities()
{
    // 实现检测事件是否包含领域实体类型的逻辑
    // 使用 NetArchTest 或 Roslyn Analyzer
}

[Fact]
public void Event_Handlers_Should_End_With_Handler_Suffix()
{
    var result = Types.InAssemblies(ModuleAssemblies)
        .That()
        .ImplementInterface(typeof(IEventHandler<>))
        .Should()
        .HaveNameEndingWith("Handler")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        "所有事件处理器必须以 'Handler' 后缀结尾 (ADR-120.4)");
}
```

---

## 依赖与相关 ADR

- **ADR-0001**：模块化单体与垂直切片架构 - 定义模块隔离和通信原则
- **ADR-0003**：命名空间与项目边界规范 - 定义命名空间映射规则
- **ADR-0005**：应用内交互模型 - 定义事件通信机制和 Handler 职责
- **ADR-0000**：架构测试元规则 - 定义架构约束的测试方法

---

## 检查清单（Checklist）

创建或修改领域事件时，检查以下项：

- [ ] 事件名称遵循 `{AggregateRoot}{Action}Event` 模式
- [ ] 动词使用过去式（如 Created、Paid、Cancelled）
- [ ] 事件包含 `Event` 后缀
- [ ] 事件定义在 `Zss.BilliardHall.Modules.{ModuleName}.Events` 命名空间
- [ ] 文件路径与命名空间严格对应
- [ ] 事件使用 `record` 定义（不可变）
- [ ] 事件属性只包含原始类型、DTO，不包含领域实体
- [ ] 事件不包含业务方法或判断逻辑
- [ ] 事件处理器命名为 `{EventName}Handler`
- [ ] 跨模块订阅仅通过事件，不直接依赖其他模块
- [ ] 如需版本演进，使用 `V{N}` 后缀
- [ ] 架构测试覆盖所有 L1 级约束

---

## 扩展落地建议

1. **代码生成模板**：
  - 创建事件和处理器的代码生成模板（如 dotnet new templates）
  - 自动生成符合规范的事件和处理器代码

2. **IDE 插件支持**：
  - 开发 IDE 插件或代码片段（snippets），快速生成标准事件
  - 提供实时命名检查和建议

3. **文档自动生成**：
  - 基于事件定义自动生成事件目录和关系图
  - 生成模块间事件订阅关系文档

4. **监控和追踪**：
  - 为事件添加追踪标识（Trace ID、Correlation ID）
  - 实现事件发布和处理的监控和日志

5. **测试支持**：
  - 提供事件测试辅助工具（Event Test Helpers）
  - 支持事件回放和时间旅行调试

---

## 版本历史

| 版本  | 日期         | 变更摘要                                                                                                                                                 |
|-----|------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1.2 | 2026-01-24 | 架构测试精化版本：1) ADR-120.2 移除"证明正确"逻辑，只保留禁止模式（架构测试哲学：裁定明显错误，不证明正确）；2) ADR-120.4 改用语义检测（接口实现）而非命名猜测，避免误杀；3) ADR-120.7 从 L1 降级至 L2（文件名约束耦合物理结构，属代码组织而非架构本体） |
| 1.1 | 2026-01-24 | 强化版本：1) 扩展 EventHandler 命名规则支持 `{Purpose}` 后缀以应对多订阅场景；2) 明确事件版本命名 ≠ 序列化兼容策略；3) 升级"动词过去式"约束从 L2 至 L1（本体语义约束）                                          |
| 1.0 | 2026-01-24 | 初始版本：定义领域事件命名规范、命名空间组织、版本演进和模块隔离约束                                                                                                                   |

---

## 附注

本 ADR 专注于领域事件的命名和组织规范，具体的事件发布、订阅机制和基础设施实现将在技术层 ADR 中定义（ADR-300 系列）。

事件的业务语义设计、事件风暴和领域建模方法不在本 ADR 范围内，应参考领域驱动设计（DDD）相关最佳实践。
