# ADR-0005：应用内交互模型与执行边界

**状态**：✅ Final（运行时宪法，不依赖具体技术栈）  
**级别**：架构根约束（Runtime Constitutional Rule）  
**适用范围**：Application / Modules / Host（不含 Platform 实现细节）  
**生效时间**：即刻  

---

## 本章聚焦内容（Focus）

本 ADR 是**运行时行为层**的核心文档，聚焦于：

1. **Use Case 执行模型**：业务用例如何被执行
2. **Handler 职责边界**：Handler 的允许和禁止行为
3. **模块通信方式**：同步 vs 异步的边界
4. **查询与命令分离**：CQRS 原则
5. **事务与一致性语义**：跨模块一致性保障
6. **错误与失败语义**：如何处理失败和异常

**不涉及**：
- ❌ 静态模块组织（见 ADR-0001）
- ❌ 启动和装配（见 ADR-0002）
- ❌ 命名空间规范（见 ADR-0003）
- ❌ 依赖包管理（见 ADR-0004）
- ❌ 架构测试机制（见 ADR-0000）

---

## 术语表（Glossary）

| 术语                  | 定义                                                                 |
|-----------------------|----------------------------------------------------------------------|
| Use Case              | 单一业务用例，最小执行单元                                           |
| Handler               | 处理业务用例的组件，是业务意图的唯一权威                             |
| CQRS                  | 命令查询职责分离，Query 只读，Command 写入                           |
| 最终一致性            | 跨模块通过异步事件达成一致，不保证强一致性                           |
| Saga                  | 跨模块补偿流程，用于处理分布式事务                                   |
| 同步调用              | 模块内允许，模块间默认禁止                                           |
| 异步通信              | 模块间默认通信方式，通过事件或消息                                   |

---

## 摘要

- **目的**：在“垂直切片 + 模块隔离”的架构前提下，定义应用在**运行时**的明确规则：
  - 谁拥有业务决策权
  - 一个请求如何被执行
  - 模块之间如何协作
  - 同步 / 异步的边界在哪里
- **核心立场**：本 ADR **不绑定任何框架或库**，只规定“行为语义”。
- **约束级别**：违反即视为架构违规，需显式破例审批。

---

## 背景

前序 ADR（0001–0004）已经解决了以下**静态问题**：

- 代码如何组织（模块、层级、命名）
- 项目如何启动（Platform / Application / Host）
- 依赖如何管理（CPM、引用规则）

但系统是否**长期可演进**，取决于运行时是否有清晰规则：

- 业务决策在哪里发生？
- Handler 能不能随意调用别的模块？
- 同步与异步的语义是否统一？

如果缺失这层约束，系统会不可避免地演化为：

- Endpoint 偷跑业务逻辑
- Handler 变成万能 Service
- 模块边界被同步调用击穿
- Saga / 事务被滥用

因此，本 ADR 定义“应用内交互模型”，作为运行时的**宪法级约束**。

---

## 决策（核心规则）

### 1. 最小执行单元：Use Case + Handler

**【必须架构测试覆盖】**

- **最小执行单元**是一个明确的业务用例（Use Case）。
- 每个 Use Case **必须对应一个 Handler**，且该 Handler 是该业务意图的**唯一权威**。
- Endpoint / Controller / Adapter 仅负责：
  - 接收外部请求
  - 映射为 Use Case
  - 转发给 Handler

**禁止**：
- 在 Endpoint 中执行业务规则
- 在多个 Handler 中分散同一业务决策

---

### 2. Handler 职责边界（不可模糊）

**【必须架构测试覆盖】**

Handler 必须满足以下条件：

**允许**：
- 执行业务规则与决策
- 协调持久化、消息发布、外部调用
- 返回结果或发布事件

**禁止**：
- 承载长期状态
- 作为跨模块同步粘合层
- 返回领域实体作为跨模块 DTO

**约束**：
- 一个 Handler 只处理一个业务意图
- Handler 应为短生命周期、无状态、可重入

---

### 3. 同步 / 异步原则（硬规则）

**【必须架构测试覆盖】**

- **模块内**：允许同步调用
- **模块间**：默认异步通信（事件 / 消息）

**明确禁止**：
- 未审批的跨模块同步调用

**例外（需审批）**：
- 明确的远程调用契约（HTTP / RPC 等）
- 必须视为不可靠远端调用（超时、失败、降级）

异步通信语义：**最终一致性**。

---

### 4. 模块通信契约（Contracts）

**【必须架构测试覆盖】**

- 模块间只能通过显式、版本化的契约通信
- 禁止引用其他模块的内部类型或领域模型

契约分层：

- Domain Event：模块内部事实
- Module Event：模块级公开契约
- Integration Event：跨系统契约（只增不改）

共享仅限 DTO，不共享实体或 VO。

---

### 5. 查询（Read）与命令（Write）

**【必须架构测试覆盖】**

- Query：只读投影 / DTO
- Command：通过 Handler 执行业务决策

**禁止**：
- 通过跨模块同步查询驱动写操作

---

### 6. 事务与一致性语义

- 不使用全局分布式事务
- 跨模块一致性通过：
  - 事件驱动
  - 幂等消费
  - 补偿流程（Saga）

Saga 仅用于：跨模块 + 跨时间 + 需补偿 的场景。

---

### 7. 错误与失败语义

- 错误必须结构化（ErrorCode + Message）
- 异步失败必须可观测（死信、告警、重试策略）

---

### 8. 强化与测试

所有运行时约束必须通过自动化架构测试验证。

**架构测试详见**：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

**执行级别分类详见**：[ADR-0005-Enforcement-Levels.md](ADR-0005-Enforcement-Levels.md)

**核心测试用例**：
- Handler 不应依赖 ASP.NET 类型
- Handler 应该是无状态的
- 模块间不应有未审批的同步调用
- 模块不应共享领域实体
- Endpoint 不应包含业务逻辑

---

### 9. 破例流程（治理）

- 所有破例必须显式记录
- 必须包含：理由、范围、期限、归还计划
- 破例默认有失效期

---

## 不遵守的后果

- 模块边界被侵蚀
- 架构退化为“同步泥球”
- 复杂度失控，重构成本指数级上升

---

## 结论

ADR-0005 定义的不是技术选型，而是**运行时秩序**：

> 谁能做决定、如何协作、失败如何承担。

只要这份 ADR 仍被遵守，底层技术可自由演进，系统仍然站得住。


---

## 与其他 ADR 关系（Related ADRs）

| ADR        | 关系                                           |
|------------|------------------------------------------------|
| ADR-0000   | 定义本 ADR 的自动化测试机制                    |
| ADR-0001   | 定义静态模块结构，本 ADR 定义运行时交互        |
| ADR-0002   | 定义启动装配，本 ADR 定义执行行为              |
| ADR-0003   | 定义命名空间规范                               |
| ADR-0004   | 定义依赖包管理规则                             |

**依赖关系**：
- 本 ADR 定义运行时行为约束
- ADR-0001 定义静态结构，本 ADR 定义动态行为
- ADR-0000 定义如何测试本 ADR 的约束

**补充文档**：
- [ADR-0005-Enforcement-Levels.md](ADR-0005-Enforcement-Levels.md)：执行级别分类

---

## 快速参考表（Quick Reference Table）

| 约束编号 | 约束描述 | 必须测试 | 测试覆盖 | ADR 章节 |
|---------|---------|---------|---------|---------|
| ADR-0005.1 | Handler 应有明确的命名约定 | ✅ | `Handlers_Should_Have_Clear_Naming_Convention` | 1, 8 |
| ADR-0005.2 | Endpoint 不应包含业务逻辑 | ✅ | `Endpoints_Should_Not_Contain_Business_Logic` | 1, 8 |
| ADR-0005.3 | Handler 不应依赖 ASP.NET 类型 | ✅ | `Handlers_Should_Not_Depend_On_AspNet` | 2, 8 |
| ADR-0005.4 | Handler 应该是无状态的 | ✅ | `Handlers_Should_Be_Stateless` | 2, 8 |
| ADR-0005.5 | 模块间不应有未审批的同步调用 | ✅ | `Modules_Should_Not_Have_Synchronous_Cross_Module_Calls` | 3, 8 |
| ADR-0005.6 | 异步方法应遵循命名约定 | ✅ | `Async_Methods_Should_Follow_Naming_Convention` | 3, 8 |
| ADR-0005.7 | 模块不应共享领域实体 | ✅ | `Modules_Should_Not_Share_Domain_Entities` | 4, 8 |
| ADR-0005.8 | Query Handler 可以返回 Contracts | ✅ | `QueryHandlers_Can_Return_Contracts` | 4, 5, 8 |
| ADR-0005.9 | Command Handler 和 Query Handler 应明确分离 | ✅ | `Command_And_Query_Handlers_Should_Be_Separated` | 5, 8 |
| ADR-0005.10 | Command Handler 不应返回业务数据 | ✅ | `CommandHandlers_Should_Not_Return_Business_Data` | 5, 8 |
| ADR-0005.11 | Handler 应使用结构化异常 | ✅ | `Handlers_Should_Use_Structured_Exceptions` | 7, 8 |
| ADR-0005.12 | 所有 Handler 应在模块程序集中 | ✅ | `All_Handlers_Should_Be_In_Module_Assemblies` | 2, 8 |

---

## 快速参考（Quick Reference）

### Use Case 执行检查清单

- [ ] 是否每个 Use Case 对应一个 Handler？
- [ ] Handler 是否是业务意图的唯一权威？
- [ ] Endpoint 是否只负责接收和转发？
- [ ] 业务逻辑是否集中在 Handler？

### Handler 职责检查清单

- [ ] Handler 是否无状态？
- [ ] Handler 是否不依赖 ASP.NET 类型？
- [ ] Handler 是否不作为跨模块同步粘合层？
- [ ] Handler 是否返回结果或发布事件？

### 模块通信检查清单

- [ ] 模块内是否使用同步调用？
- [ ] 模块间是否使用异步事件？
- [ ] 跨模块同步调用是否经过审批？
- [ ] 是否避免了共享领域模型？

### CQRS 检查清单

- [ ] Query 是否只读？
- [ ] Command 是否通过 Handler 执行？
- [ ] 是否避免了通过查询驱动写操作？

### 一致性检查清单

- [ ] 是否避免了全局分布式事务？
- [ ] 跨模块一致性是否通过事件驱动？
- [ ] 是否实现了幂等消费？
- [ ] Saga 是否只用于必要场景？

---

## 常见问题（FAQ）

### Q: 什么时候可以跨模块同步调用？

**A:** 
1. 必须经过架构审批
2. 必须视为远程调用（超时、失败、降级）
3. 必须记录在破例表中

---

### Q: Query 可以返回 Contracts 吗？

**A:** 
是的，Query Handler 可以返回 Contracts（只读 DTO）。
但 Command Handler 不能依赖 Contracts 做业务决策。

---

### Q: 如何处理跨模块事务？

**A:** 
1. 不使用全局分布式事务
2. 通过事件驱动达成最终一致性
3. 如需补偿，使用 Saga 模式
4. 确保幂等消费

---

### Q: Handler 能有多复杂？

**A:** 
1. 关注点：业务规则和决策
2. 建议：单个 Handler 不超过 100 行
3. 如果过于复杂，考虑拆分为多个 Use Case
