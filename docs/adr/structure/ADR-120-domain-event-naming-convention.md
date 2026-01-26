# ADR-120：领域事件命名规范

**状态**：✅ 已采纳（Adopted）  
**版本**：1.0
**级别**：结构约束（Structure Constraint）  
**适用范围**：所有模块（Modules）、领域事件定义、事件处理器  
**生效时间**：即刻  
**依赖 ADR**：ADR-0001、ADR-0003、ADR-0005

---

## 聚焦内容（Focus）

- 统一事件命名：动词过去式 + Event 后缀
- 强制命名空间组织：Modules.{Name}.Events
- 版本演进标准：V{N} 后缀
- 隔离约束：禁止领域实体、禁止业务方法

---

## 术语表（Glossary）

| 术语         | 定义                       |
|------------|--------------------------|
| 领域事件       | 描述已发生业务事实的不可变对象          |
| 集成事件       | 跨系统事件，由领域事件转换            |
| 事件命名空间     | 必须与物理文件结构对应              |
| 事件版本       | V{N} 格式，用于向后兼容演进         |
| 事件聚合根      | 事件所属聚合根实体，决定业务语义边界       |

---

## 决策（Decision）

### 事件命名规则

**命名模式**：`{AggregateRoot}{Action}Event[{Version}]`

- ❌ 禁止缺少 Event 后缀
- ❌ 禁止使用现在时/进行时/原形动词
- ❌ 禁止嵌入跨模块语义
- ✅ 必须使用动词过去式

**示例**：

```csharp
// ✅ 正确
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);
public record OrderPaidEvent(Guid OrderId, decimal Amount, DateTime PaidAt);

// ❌ 错误
public record OrderCreated(Guid OrderId);  // 缺少 Event 后缀
public record OrderCreating(Guid OrderId);  // 进行时
public record OrderCreateEvent(Guid OrderId);  // 动词原形
```

---

### 命名空间组织

**命名空间规则**：`Zss.BilliardHall.Modules.{ModuleName}.Events[.{SubNamespace}]`

- ❌ 禁止在 Domain 或其他非标准命名空间
- ❌ 禁止使用 Shared/Common 命名空间
- ✅ 必须在模块 Events 命名空间下

**文件结构**：

```
src/Modules/{ModuleName}/
  ├── Events/
  │    ├── {Event}Event.cs        (一事件一文件)
  └── EventHandlers/
       ├── {Event}Handler.cs      (基础模式)
       └── {Event}{Purpose}Handler.cs  (扩展模式)
```

---

### 事件处理器命名

**命名模式**：
- 基础：`{EventName}Handler`
- 扩展：`{EventName}{Purpose}Handler`（多订阅场景）

- ❌ 禁止使用 Processor、Service 后缀
- ❌ 禁止命名与事件不对应
- ✅ Purpose 必须清晰描述业务意图

**示例**：

```csharp
// ✅ 基础模式
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent> { }

// ✅ 扩展模式
public class OrderPaidEventAddPointsHandler : IEventHandler<OrderPaidEvent> { }
public class OrderPaidEventGenerateInvoiceHandler : IEventHandler<OrderPaidEvent> { }

// ❌ 错误
public class OrderCreatedProcessor { }  // 错误后缀
public class OrderPaidEventHandler1 { }  // Purpose 不清晰
```

---

### 事件内容约束

**允许**：
- 原始类型（Guid、int、string、DateTime）
- DTO（只读数据对象）
- record 定义（不可变）

**禁止**：
- ❌ 领域实体（Entity/Aggregate/ValueObject）
- ❌ 业务方法
- ❌ 业务判断逻辑

**示例**：

```csharp
// ✅ 正确
public record OrderCreatedEvent(
    Guid OrderId,
    Guid MemberId,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);
public record OrderItemDto(string ProductId, int Quantity, decimal Price);

// ❌ 错误
public record OrderCreatedEvent(Order Order, Member Member);  // 领域实体
public record OrderCreatedEvent(Guid OrderId)
{
    public bool CanBeCancelled() => ...;  // 业务方法
}
```

---

### 版本演进

**版本标识**：`V{N}`（N 从 2 开始）

- ❌ 禁止直接修改现有事件
- ✅ 必须提供转换适配器
- ⚠️  类型版本 ≠ 序列化兼容性（在 ADR-300 系列定义）

**示例**：

```csharp
// V1（不带版本后缀）
public record OrderCreatedEvent(Guid OrderId, Guid MemberId, DateTime CreatedAt);

// V2
public record OrderCreatedEventV2(
    Guid OrderId, Guid MemberId, DateTime CreatedAt,
    string Source, string Channel
);

// 转换适配器
public class OrderCreatedEventAdapter
{
    public static OrderCreatedEventV2 ToV2(OrderCreatedEvent v1) =>
        new(v1.OrderId, v1.MemberId, v1.CreatedAt, "Unknown", "Web");
}
```

---

## 快速参考表（Quick Reference）

| 约束编号      | 描述                     | 层级 | 测试用例                                      | 章节       |
|-----------|------------------------|----|--------------------------------------------|----------|
| ADR-120.1 | 事件必须以 Event 后缀结尾      | L1 | Event_Types_Should_End_With_Event_Suffix   | 事件命名规则   |
| ADR-120.2 | 事件名称必须使用动词过去式          | L1 | Event_Names_Should_Use_Past_Tense_Verbs    | 事件命名规则   |
| ADR-120.3 | 事件必须在模块 Events 命名空间下  | L1 | Events_Should_Be_In_Events_Namespace       | 命名空间组织   |
| ADR-120.4 | 事件处理器必须以 Handler 后缀结尾 | L1 | Event_Handlers_Should_End_With_Handler_Suffix | 事件处理器命名  |
| ADR-120.5 | 事件不得包含领域实体类型           | L1 | Events_Should_Not_Contain_Domain_Entities  | 事件内容约束   |
| ADR-120.6 | 事件不得包含业务方法             | L1 | Events_Should_Not_Contain_Business_Methods | 事件内容约束   |
| ADR-120.7 | 事件文件名必须与类型名一致          | L2 | 人工 Code Review                             | 命名空间组织   |
| ADR-120.8 | 事件版本标识使用 V{N} 格式       | L2 | Event_Versions_Should_Use_VN_Format        | 版本演进     |

> L1: 架构测试覆盖（CI 自动阻断），L2: Code Review 或启发式检查

---

## 依赖与相关 ADR

| 关联 ADR   | 关系          |
|----------|-------------|
| ADR-0001 | 定义模块隔离和通信原则 |
| ADR-0003 | 定义命名空间映射规则  |
| ADR-0005 | 定义事件通信机制    |
| ADR-0000 | 定义架构约束测试方法  |

---

## 检查清单（Checklist）

- [ ] 事件名称遵循 {AggregateRoot}{Action}Event 模式
- [ ] 动词使用过去式
- [ ] 事件在 Zss.BilliardHall.Modules.{ModuleName}.Events 命名空间
- [ ] 文件路径与命名空间严格对应
- [ ] 事件使用 record 定义
- [ ] 事件只包含原始类型、DTO
- [ ] 事件不包含业务方法
- [ ] 事件处理器命名为 {EventName}Handler
- [ ] 跨模块订阅仅通过事件
- [ ] 版本演进使用 V{N} 后缀
- [ ] L1 级约束有架构测试覆盖

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：

- 事件发布/订阅基础设施实现（ADR-300 系列）
- 事件序列化兼容策略（ADR-300 系列）
- 事件风暴和领域建模方法（DDD 最佳实践）
- 事件存储和回放机制
- 代码生成模板和 IDE 插件

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 事件命名基于 CQRS 和事件驱动模式
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - 命名约定遵循术语规范
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)

**被依赖（Depended By）**：
- [ADR-210：事件版本化与兼容性](../runtime/ADR-210-event-versioning-compatibility.md) - 事件版本化依赖命名约定
- [ADR-220：事件总线集成规范](../runtime/ADR-220-event-bus-integration.md) - 事件总线集成依赖事件命名规范

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-121：契约 DTO 命名与组织](./ADR-121-contract-dto-naming-organization.md) - 同为命名规范

---

## 版本历史

| 版本  | 日期         | 变更摘要                                                                                           |
|-----|------------|------------------------------------------------------------------------------------------------|
| 1.2 | 2026-01-24 | 精简版本：移除冗长说明，只保留规则本体；ADR-120.2 升级至 L1；ADR-120.7 降级至 L2；强化事件处理器命名规则支持多订阅场景；明确类型版本 ≠ 序列化兼容 |
| 1.1 | 2026-01-24 | 强化版本：扩展 EventHandler 命名规则；明确版本命名不等于序列化策略；升级动词过去式约束至 L1                                       |
| 1.0 | 2026-01-24 | 初始版本                                                                                           |
