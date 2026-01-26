# ADR-0005：应用内交互模型极简判裁版

**状态**：✅ Final（仅保留裁决性规则，无“建议/经验”）  
**版本**：1.0
**级别**：根约束（Runtime Constitutional Rule）  
**适用范围**：Application / Modules / Host  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

仅定义适用于全生命周期自动化裁决/阻断的**运行时交互约束**：

- Use Case + Handler = 最小业务决策单元
- Handler 职责、边界、合规输出
- 模块通信（同步/异步、契约形式）硬约束
- CQRS 强制分离与唯一权威
- 禁止共享领域模型、未审批同步、endpoint/handler 角色混淆

---

## 术语表（Glossary）

| 术语            | 定义             |
|---------------|----------------|
| Use Case      | 端到端业务用例        |
| Handler       | 业务用例的唯一决策实现    |
| Command/Query | 分别代表写/读单一职责    |
| CQRS          | 命令-查询职责分离      |
| 合约（Contract）  | 模块间只读通信对象      |
| 领域实体          | 业务内聚的复杂类型，不跨模块 |
| 模块间通信         | 只允许事件、合约、原始类型  |

---

## 决策（Decision）

### Use Case 执行与裁决权

- 每个业务用例必须唯一 Handler，且该 Handler 拥有全部业务决策权
- Endpoint/Controller 仅允许做请求适配和 Handler 调用，禁止承载任何业务规则

### Handler 职责边界

- Handler 不得持有跨调用生命周期的业务状态
- Handler 禁止作为同步跨模块"粘合层"
- Handler 不允许返回或暴露领域实体作为出参

### 模块通信及同步/异步边界

- 模块内：允许同步方法调用
- 模块间：**默认只能异步通信（领域事件/集成事件）**
- 未经审批，禁止任何跨模块同步调用（如直接方法、同步仓储、接口依赖等）

### 通信契约（Contract）与领域模型隔离

- 模块间仅通过协定 DTO/事件通信，禁止直接引用/传递 Entity/Aggregate/VO
- 合约（Contract）只允许传递数据，不承载业务决策/行为

### CQRS & Handler 唯一性

- Command Handler 只允许执行业务逻辑并返回 void 或唯一标识
- Query Handler 只允许只读 DTO/投影返回
- Command/Query Handler 必须职责分离，不允许合并、混用

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - CQRS 模式基于模块隔离和垂直切片
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - Handler 装配基于三层体系
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - Handler 组织遵循命名空间规范
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 依赖管理遵循包管理规则

**被依赖（Depended By）**：
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 事件命名基于 CQRS 模式
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - DTO 组织基于 CQRS 分离

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

## 必测/必拦架构测试（Enforcement）

- Handler 必须无状态、无领域持久字段
- Endpoint 不得出现业务规则（有状态/决策分支/存储/调用其他模块等）
- Handler/Endpoint 不直接或间接依赖 ASP.NET Host 类型
- 跨模块同步调用自动检测并阻断
- Contracts 类型仅允许原始类型、DTO，不允许 Entity/VO
- 所有 Handler 均只在模块程序集，不允横向注册
- Query Handler 不允许写入操作，Command Handler 不允许查询/返回实体

所有规则一律用 NetArchTest、Roslyn Analyzer 或人工 Gate 强制，有一项违规视为架构违规。

---

## 极简检查清单（Checklist）

- [ ] 每个用例唯一 Handler 归属
- [ ] Endpoint 禁止写业务条件/分支/存储
- [ ] Handler/Endpoint 不依赖 ASP.NET 类型
- [ ] Handler 无持久状态
- [ ] 跨模块所有调用为异步事件/契约
- [ ] Contract 不含业务判断、行为方法
- [ ] Query/Command Handler 职责完全分离

---

## 约束关系与修订边界

- 本 ADR 仅列出不可破例自动化裁断项
- 所有治理建议、失败处理、Saga/补偿、最佳实践类内容须单独写 Guide/Rationale
- 修订仅允许缩减内容或提升自动裁决性，禁止添加“仅建议性”内容
- 审判权来自自动化检测，可扩充测试工具，但不得新增人工经验条款

---

## 快速参考表

| 约束编号   | 约束描述                  | 测试方式            | 测试用例                                           | 必须遵守 |
|--------|----------------------- |-----------------|------------------------------------------------|------|
| 0005.1 | Handler 命名规范清晰        | L1 - NetArchTest | Handlers_Should_Have_Clear_Naming_Convention    | ✅    |
| 0005.2 | Endpoint 不含业务逻辑       | L2 - 语义检查        | Endpoints_Should_Not_Contain_Business_Logic     | ✅    |
| 0005.3 | Handler 不依赖 ASP.NET   | L1 - NetArchTest | Handlers_Should_Not_Depend_On_AspNet            | ✅    |
| 0005.4 | Handler 无状态           | L1 - NetArchTest | Handlers_Should_Be_Stateless                    | ✅    |
| 0005.5 | 跨模块同步调用禁止             | L2 - 语义检查        | Modules_Should_Not_Have_Synchronous_Cross_Module_Calls | ✅    |
| 0005.6 | 异步方法遵循命名规范            | L1 - NetArchTest | Async_Methods_Should_Follow_Naming_Convention   | ✅    |
| 0005.7 | 模块不共享领域实体             | L1 - NetArchTest | Modules_Should_Not_Share_Domain_Entities        | ✅    |
| 0005.8 | Query Handler 可返回契约   | L1 - NetArchTest | QueryHandlers_Can_Return_Contracts              | ✅    |
| 0005.9 | Command/Query 职责分离    | L1 - NetArchTest | Command_And_Query_Handlers_Should_Be_Separated  | ✅    |
| 0005.10 | Command Handler 不返回业务数据 | L1 - NetArchTest | CommandHandlers_Should_Not_Return_Business_Data | ✅    |
| 0005.11 | Handler 使用结构化异常       | L2 - 语义检查        | Handlers_Should_Use_Structured_Exceptions       | ✅    |
| 0005.12 | Handler 仅在模块程序集       | L1 - NetArchTest | All_Handlers_Should_Be_In_Module_Assemblies     | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests），L2=语义半自动（Roslyn/启发式）

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0005_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---

## 版本历史

| 版本  | 日期         | 变更说明             |
|-----|------------|------------------|
| 4.0 | 2026-01-26 | 同步测试映射，格式统一化  |
| 3.0 | 2026-01-23 | 精简为极简判裁版，仅保裁决性规则 |
| 2.0 | 2026-01-20 | 作为“经验/治理+裁决”混合版  |
| 1.0 | 2026-01-20 | 初始发布             |

---

## 附注

本文件禁止添加示例/建议/FAQ，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、最佳实践、Saga/补偿模式）请查阅：
- [ADR-0005 Copilot Prompts](../../copilot/adr-0005.prompts.md)
- Guide/FAQ/Rationale 另行维护，不具裁决权力
