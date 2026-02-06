---
adr: ADR-120
title: "领域事件命名规范"
status: Final
level: Structure
version: "2.2"
deciders: "Architecture Board"
date: 2026-02-06
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-120：领域事件命名规范

**适用范围**：所有模块（Modules）、领域事件定义、事件处理器  
## Focus（聚焦内容）

- 统一事件命名：动词过去式 + Event 后缀
- 强制命名空间组织：Modules.{Name}.Events
- 版本演进标准：V{N} 后缀
- 隔离约束：禁止领域实体、禁止业务方法

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|--------------------------|----------------------|
| 领域事件       | 描述已发生业务事实的不可变对象          | Domain Event      |
| 集成事件       | 跨系统事件，由领域事件转换            | Integration Event |
| 事件命名空间     | 必须与物理文件结构对应              | Event Namespace   |
| 事件版本       | V{N} 格式，用于向后兼容演进         | Event Version     |
| 事件聚合根      | 事件所属聚合根实体，决定业务语义边界       | Event Aggregate Root |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-120 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-120_<Rule>_<Clause>
> ```

---

### ADR-120_1：事件类型命名规范（Rule）

#### ADR-120_1_1 事件命名模式强制要求

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

#### ADR-120_1_2 事件命名空间组织规范

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

---

### ADR-120_2：事件处理器命名规范（Rule）

#### ADR-120_2_1 事件处理器命名模式

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

---

### ADR-120_3：事件内容约束（Rule）

#### ADR-120_3_1 事件内容类型约束

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
```

---

#### ADR-120_3_2 事件行为约束

**禁止在事件中包含业务方法**：

```csharp
// ❌ 错误
public record OrderCreatedEvent(Guid OrderId)
{
    public bool CanBeCancelled() => ...;  // 业务方法
}

// ✅ 正确
public record OrderCreatedEvent(Guid OrderId, DateTime CreatedAt);
```

---

---

### ADR-120_4：事件版本演进规范（Rule）

#### ADR-120_4_1 版本命名规范

**版本标识**：`V{N}`（N 从 2 开始）

- ❌ 禁止直接修改现有事件
- ✅ 必须提供转换适配器
- ⚠️  类型版本 ≠ 序列化兼容性（在 ADR-210 系列定义）

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

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-120 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-120_1_1** | L1 | ArchitectureTests 验证事件命名模式 | §ADR-120_1_1 |
| **ADR-120_1_2** | L1 | ArchitectureTests 验证事件命名空间 | §ADR-120_1_2 |
| **ADR-120_2_1** | L1 | ArchitectureTests 验证事件处理器命名 | §ADR-120_2_1 |
| **ADR-120_3_1** | L1 | ArchitectureTests 验证事件内容类型 | §ADR-120_3_1 |
| **ADR-120_3_2** | L1 | ArchitectureTests 验证事件行为约束 | §ADR-120_3_2 |
| **ADR-120_4_1** | L1 | ArchitectureTests 验证版本命名规范 | §ADR-120_4_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - 事件命名基于 CQRS 和事件驱动模式
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - 命名约定遵循术语规范
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)

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

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 2.2 | 2026-02-06 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。将原有规则智能分组为 4 个 Rule、6 个 Clause，并创建完整的 Enforcement 映射表 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
