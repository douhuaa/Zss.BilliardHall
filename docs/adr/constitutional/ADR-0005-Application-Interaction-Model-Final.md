---
adr: ADR-0005
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


# ADR-0005：应用内交互模型极简判裁版

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

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - CQRS 模式基于模块隔离和垂直切片
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - Handler 装配基于三层体系
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - Handler 组织遵循命名空间规范
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 依赖管理遵循包管理规则

**被依赖（Depended By）**：
- [ADR-905：执行级别分类](../../governance/ADR-905-enforcement-level-classification.md) - 执行级别补充文档依赖本 ADR
- [ADR-920：示例代码治理宪法](../../governance/ADR-920-examples-governance-constitution.md) - 示例治理依赖 CQRS 模式
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


- 待补充


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 同步测试映射，格式统一化                                 |
