# ADR-220：事件总线集成规范

> ⚖️ **本 ADR 定义事件总线集成的架构约束，确保模块间通信的松耦合和可靠性。**

**状态**：✅ Accepted  
**级别**：运行时层（Runtime Constraint）  
**适用范围**：所有模块间事件通信  
**生效时间**：待审批通过后  
**依赖 ADR**：ADR-0001（模块化单体与垂直切片架构）

---

## 聚焦内容（Focus）

- 事件总线抽象与依赖隔离
- 至少一次传递保证
- 同步等待禁止约束
- 事件订阅者生命周期
- 跨模块数据契约约束
- 事件总线测试执法

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| 事件总线 | 模块间异步通信的基础设施 | Event Bus |
| 至少一次传递 | 消息至少会被传递一次，可能重复 | At-Least-Once Delivery |
| Outbox Pattern | 通过数据库事务保证消息可靠发送的模式 | Outbox Pattern |
| 发送即忘记 | 发送消息后不保证送达的模式 | Fire-and-Forget |
| 数据契约 | 只读、版本化的跨模块数据传输对象 | Data Contract / DTO |
| 幂等性 | 重复执行产生相同结果的特性 | Idempotency |

---

## 决策（Decision）

### 禁止直接依赖具体事件总线（ADR-220.1）【必须架构测试覆盖】

**规则**：
- 模块代码禁止直接依赖具体事件总线实现
- 强制要求：
  - ✅ 必须通过 `IEventBus` 抽象接口
  - ✅ 事件总线实现在 Platform 或 Infrastructure 层
  - ❌ 禁止在模块中引用 `Wolverine.*`、`Kafka.*` 等包
  - ❌ 禁止直接使用消息队列客户端

**判定**：
- ❌ 模块代码引用具体事件总线包
- ❌ 模块代码直接使用 MassTransit/NServiceBus/Kafka 客户端
- ✅ 模块仅依赖 `IEventBus` 抽象

### 至少一次传递保证（ADR-220.2）【必须架构测试覆盖】

**规则**：
- 事件发布必须保证至少一次传递
- 实现要求：
  - ✅ 使用 Outbox Pattern 或等效机制
  - ✅ 事件持久化到数据库
  - ✅ 支持重试机制
  - ❌ 禁止"发送即忘记"（Fire-and-Forget）
  
- 容忍场景：
  - 订阅者必须实现幂等性处理重复事件

**判定**：
- ❌ 事件发布未持久化
- ❌ 无重试机制
- ❌ 使用 Fire-and-Forget 模式
- ✅ 实现 Outbox Pattern 或等效机制

### 禁止同步等待事件处理（ADR-220.3）【必须架构测试覆盖】

**规则**：
- 调用方禁止在发布事件后等待事件处理结果
- 事件发布必须是真正的异步操作
- 禁止在发布后通过轮询或回调等待结果

**判定**：
- ❌ 发布事件后等待处理结果
- ❌ 发布事件后轮询状态
- ❌ 使用回调机制等待事件完成
- ✅ 发布事件后立即返回

### 事件订阅者生命周期（ADR-220.4）【必须架构测试覆盖】

**规则**：
- 事件订阅者必须注册为 Scoped 或 Transient 生命周期
- 生命周期规则：
  - ✅ Scoped（推荐）：每个事件处理有独立实例
  - ✅ Transient：每次注入创建新实例
  - ❌ 禁止 Singleton：避免状态污染

**判定**：
- ❌ EventHandler 注册为 Singleton
- ✅ EventHandler 注册为 Scoped
- ✅ EventHandler 注册为 Transient

### 跨模块事件数据契约（ADR-220.5）【必须架构测试覆盖】

**规则**：
- 跨模块事件必须仅包含数据契约或原始类型
- 允许的数据类型：
  - ✅ 原始类型（int、string、Guid、DateTime）
  - ✅ 数据契约（DTO）
  - ✅ 值对象（如 Money、Address）
  
- 禁止的数据类型：
  - ❌ 领域实体
  - ❌ 聚合根
  - ❌ 领域服务

**判定**：
- ❌ 事件包含领域实体或聚合根
- ❌ 事件包含领域服务引用
- ✅ 事件仅包含原始类型和 DTO

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

### 执行说明

**L1 测试**：
- 检测模块是否依赖具体事件总线包
- 验证 EventHandler 注册生命周期
- 检查事件是否包含领域实体

**L2 测试**：
- 架构审查验证 Outbox Pattern 实现
- Code Review 检查是否存在同步等待
- 集成测试验证至少一次传递

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
- 标注影响范围和风险

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **运行时层 ADR**
  * 修改需 Tech Lead/架构师审批
  * 更换事件总线实现不需要修改本 ADR
  * 必须更新相关架构测试

### 失效与替代

* 如有更优方案，可创建 ADR-22X 替代本 ADR
* 被替代后，本 ADR 状态改为 Superseded

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 具体的事件总线选型（Wolverine/Kafka/RabbitMQ）
- ✗ 消息队列的运维配置
- ✗ 事件重放策略
- ✗ 事件的持久化存储方案
- ✗ Outbox Pattern 的具体实现细节

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-210：领域事件版本化与兼容性](ADR-210-event-versioning-compatibility.md)

### 技术资源
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [At-Least-Once Delivery](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions)

### 实践指导
- 事件总线配置示例参见 `docs/copilot/adr-0220.prompts.md`

---


## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 事件总线集成依赖事件命名规范
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 事件总线是模块间通信的实现

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-210：事件版本化与兼容性](./ADR-210-event-versioning-compatibility.md) - 事件序列化和版本化

---


## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 2.0 | 2026-01-25 | 重构为裁决型格式，添加决策章节 | GitHub Copilot |
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
