# ADR-220：集成事件总线选型与适配规范

**状态**：Draft  
**级别**：运行时层  
**影响范围**：所有模块间事件通信  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-220.1：模块禁止直接依赖具体事件总线实现

模块代码**禁止**直接依赖具体的事件总线实现（如 Wolverine、Kafka、RabbitMQ）。

**强制要求**：
- ✅ 必须通过 `IEventBus` 抽象接口
- ✅ 事件总线实现在 Platform 或 Infrastructure 层
- ❌ 禁止在模块中引用 `Wolverine.*`、`Kafka.*` 等包
- ❌ 禁止直接使用消息队列客户端

### ADR-220.2：事件发布必须支持至少一次传递保证

事件发布机制**必须**保证至少一次传递（At-Least-Once Delivery）。

**实现要求**：
- ✅ 使用 Outbox Pattern 或等效机制
- ✅ 事件持久化到数据库
- ✅ 支持重试机制
- ❌ 禁止"发送即忘记"（Fire-and-Forget）

**容忍场景**：
- 订阅者必须实现幂等性处理重复事件

### ADR-220.3：禁止在同步流程中等待事件处理结果

调用方**禁止**在发布事件后等待事件处理结果。

**禁止模式**：
```csharp
// ❌ 错误：同步等待
await _eventBus.Publish(new OrderCreated(...));
var result = await WaitForOrderProcessed(); // 禁止
```

**正确模式**：
```csharp
// ✅ 正确：异步发布，不等待
await _eventBus.Publish(new OrderCreated(...));
return orderId; // 立即返回
```

### ADR-220.4：事件订阅者必须注册为 Scoped 或 Transient

事件订阅者（EventHandler）**必须**注册为 Scoped 或 Transient 生命周期。

**生命周期规则**：
- ✅ Scoped（推荐）：每个事件处理有独立实例
- ✅ Transient：每次注入创建新实例
- ❌ 禁止 Singleton：避免状态污染

### ADR-220.5：跨模块事件必须通过数据契约传递数据

跨模块事件**必须**仅包含数据契约（DTO）或原始类型，不得包含领域对象。

**允许的数据类型**：
- ✅ 原始类型（int、string、Guid、DateTime）
- ✅ 数据契约（DTO）
- ✅ 值对象（如 Money、Address）
- ❌ 领域实体
- ❌ 聚合根
- ❌ 领域服务

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-220.1 | L1 | `Modules_Must_Not_Depend_On_Concrete_EventBus` |
| ADR-220.2 | L2 | 架构审查 + 集成测试 |
| ADR-220.3 | L2 | Code Review |
| ADR-220.4 | L1 | `EventHandlers_Must_Be_Scoped_Or_Transient` |
| ADR-220.5 | L1 | `Events_Must_Not_Contain_Domain_Entities` |

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **性能关键路径**：经过压测证明异步导致不可接受的延迟
2. **外部系统约束**：第三方系统要求同步响应
3. **迁移期**：从同步架构向异步架构过渡

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 提供性能测试数据
- 指定迁移计划（不超过 6 个月）

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **运行时层 ADR**
  * 修改需 Tech Lead/架构师审批
  * 更换事件总线实现不需要修改本 ADR

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 具体的事件总线选型（Wolverine/Kafka/RabbitMQ）
- ✗ 消息队列的运维配置
- ✗ 事件重放策略
- ✗ 事件的持久化存储方案

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-0001：模块化单体与垂直切片架构
- ADR-210：领域事件版本化与兼容性

### 技术资源
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [At-Least-Once Delivery](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions)

### 实践指导
- 事件总线配置示例参见 `docs/copilot/adr-0220.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
