---
adr: ADR-005
title: "应用内交互模型与执行边界"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-01-29
version: "2.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-005：应用内交互模型极简判裁版

> ⚖️ **本 ADR 是架构宪法的核心，定义应用内交互模型的唯一裁决源。**

---
## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**运行时交互约束**：

- Use Case + Handler = 最小业务决策单元
- Handler 职责、边界、合规输出
- 模块通信（同步/异步、契约形式）硬约束
- CQRS 强制分离与唯一权威
- 禁止共享领域模型、未审批同步、endpoint/handler 角色混淆

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|---------------|----------------|--------------|
| Use Case      | 端到端业务用例        | Use Case     |
| Handler       | 业务用例的唯一决策实现    | Handler      |
| Command/Query | 分别代表写/读单一职责    | Command/Query |
| CQRS          | 命令-查询职责分离      | CQRS         |
| 合约（Contract）  | 模块间只读通信对象      | Contract     |
| 领域实体          | 业务内聚的复杂类型，不跨模块 | Domain Entity |
| 模块间通信         | 只允许事件、合约、原始类型  | Inter-Module Communication |

---

---

## Decision（裁决）

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

---

## Enforcement（执法模型）

- Handler 必须无状态、无领域持久字段
- Endpoint 不得出现业务规则（有状态/决策分支/存储/调用其他模块等）
- Handler/Endpoint 不直接或间接依赖 ASP.NET Host 类型
- 跨模块同步调用自动检测并阻断
- Contracts 类型仅允许原始类型、DTO，不允许 Entity/VO
- 所有 Handler 均只在模块程序集，不允横向注册
- Query Handler 不允许写入操作，Command Handler 不允许查询/返回实体

所有规则一律用 NetArchTest、Roslyn Analyzer 或人工 Gate 强制，有一项违规视为架构违规。

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **Handler 内部实现细节**：不约束 Handler 内部如何组织代码、使用哪些设计模式（Strategy、Factory 等）
- **具体 ORM 或数据访问技术**：不约束使用 Entity Framework、Dapper 还是其他数据访问技术
- **事件存储实现**：不规定领域事件是否持久化、使用何种事件存储方案（EventStore、数据库等）
- **DTO 序列化格式**：不约束 DTO 使用 JSON、Protobuf 还是其他序列化格式
- **API 版本化策略**：不涉及 API 如何版本化（URI、Header、MediaType 等）
- **身份验证和授权**：不管理认证机制（JWT、OAuth）或授权策略（基于角色、策略等）
- **日志记录格式**：不约束日志的具体格式、日志级别或日志聚合方式
- **性能监控和追踪**：不涉及 APM 工具选型、分布式追踪实现（OpenTelemetry、Jaeger 等）
- **错误码和错误消息**：不规定错误的编码规范、多语言支持或用户友好消息格式
- **事件重试和补偿**：不约束事件失败后的重试策略、补偿事务或 Saga 模式实现

---

## Prohibited（禁止行为）


以下行为明确禁止：

### Handler 职责违规

- ❌ **Handler 持有状态**：Handler 禁止使用实例字段存储业务状态（除依赖注入的服务外）
- ❌ **Handler 作为跨模块粘合层**：Handler 禁止同步调用多个其他模块的 Handler 并聚合结果
- ❌ **Handler 返回领域实体**：Handler 禁止将 Entity、Aggregate、ValueObject 直接作为返回值
- ❌ **Handler 直接依赖 HttpContext**：Handler 禁止依赖 `HttpContext`、`ClaimsPrincipal` 等 ASP.NET 类型

### 模块通信违规

- ❌ **未审批的跨模块同步调用**：禁止直接调用其他模块的 Handler、Repository 或服务（必须通过事件）
- ❌ **共享领域对象**：禁止直接传递或返回其他模块的 Entity/Aggregate/VO
- ❌ **直接引用其他模块类型**：禁止通过 `using` 引用其他模块的内部类型（除 Contracts 外）
- ❌ **同步查询其他模块数据**：禁止通过同步 Repository 或数据库视图查询其他模块的数据

### CQRS 分离违规

- ❌ **Command Handler 返回业务数据**：Command Handler 只能返回 `void`、`Task`、`Unit` 或唯一标识符（Guid/Id）
- ❌ **Query Handler 执行写操作**：Query Handler 禁止调用 Repository 的 Add/Update/Delete 方法
- ❌ **Command 和 Query 混用**：禁止在同一个 Handler 中同时处理 Command 和 Query

### 契约设计违规

- ❌ **Contract 包含业务逻辑**：Contract DTO 禁止包含计算属性、业务判断方法或行为
- ❌ **Contract 携带领域对象**：事件或 DTO 禁止直接包含 Entity/Aggregate/VO 类型
- ❌ **Endpoint 包含业务规则**：Endpoint/Controller 禁止包含 if/switch 等业务决策逻辑（应委托给 Handler）


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - CQRS 模式基于模块隔离和垂直切片
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - Handler 装配基于三层体系
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - Handler 组织遵循命名空间规范
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 依赖管理遵循包管理规则

**被依赖（Depended By）**：
- [ADR-905：执行级别分类](../../governance/ADR-905-enforcement-level-classification.md) - 执行级别补充文档依赖本 ADR
- [ADR-920：示例代码治理规范](../../governance/ADR-920-examples-governance-constitution.md) - 示例治理依赖 CQRS 模式
- [ADR-201：Handler 生命周期管理](../../runtime/ADR-201-handler-lifecycle-management.md) - Handler 管理基于 CQRS
- [ADR-210：领域事件版本化与兼容性](../../runtime/ADR-210-event-versioning-compatibility.md) - 事件版本化基于 CQRS
- [ADR-220：事件总线集成规范](../../runtime/ADR-220-event-bus-integration.md) - 事件总线基于 CQRS
- [ADR-240：Handler 异常约束](../../runtime/ADR-240-handler-exception-constraints.md) - Handler 异常处理基于 CQRS
- [ADR-124：Endpoint 命名及参数约束规范](../../structure/ADR-124-endpoint-naming-constraints.md) - Endpoint 基于 CQRS
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 事件命名基于 CQRS 模式
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - DTO 组织基于 CQRS 分离

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

---

## References（非裁决性参考）

**CQRS 与领域驱动设计**：
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html) - CQRS 模式权威解释
- [Greg Young - CQRS Documents](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf) - CQRS 模式详细文档
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/) - 领域驱动设计经典著作
- [Implementing Domain-Driven Design by Vaughn Vernon](https://vaughnvernon.com/?page_id=168) - DDD 实践指南

**消息传递与事件驱动**：
- [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/) - 企业集成模式参考
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html) - 事件驱动架构概述
- [Messaging Patterns](https://www.enterpriseintegrationpatterns.com/patterns/messaging/) - 消息传递模式

**相关内部文档**：
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - 了解模块隔离原则
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 了解三层装配体系
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 了解依赖管理规则
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 事件命名规范
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - DTO 组织规范
- [ADR-201：Handler 生命周期管理](../runtime/ADR-201-handler-lifecycle-management.md) - Handler 生命周期详解


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 同步测试映射，格式统一化                                 |
