---
adr: ADR-0006
title: "术语与编号宪法"
status: Final
level: Constitutional
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-23
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-0006：术语与编号宪法

> ⚖️ **本 ADR 是所有 ADR 的元规则，定义术语语义和编号体系的唯一裁决源。**

**状态**：✅ Final（裁决型ADR）  
## Focus（聚焦内容）

- 架构术语的唯一权威定义（Glossary）
- ADR 编号分层规则与语义约束
- 编号格式标准与一致性要求
- 编号语义边界与禁止跨界
- 违规判定依据

---

---

## Glossary（术语表）

本节定义本架构体系中所有核心术语的**唯一权威含义**。

### 架构组织术语

| 术语 | 定义 | 英文对照 |
|---------------|------------------------------------------------------|--------------------|
| ADR           | Architecture Decision Record，架构决策记录，定义架构约束的宪法级文档      | ADR                |
| 宪法层 ADR       | ADR-0001~0009，系统根基，不可推翻，只能细化，破例需特批                  | Constitutional ADR |
| 治理层 ADR       | ADR-0000, 900~999，架构过程、测试、CI、审批、破例管理 | Governance ADR     |
| 结构层 ADR       | ADR-100~199，对宪法层结构约束的细化和补充（命名、组织、静态边界）              | Structure ADR      |
| 运行层 ADR       | ADR-200~299，对宪法层运行时约束的细化和补充（生命周期、事件、异常）             | Runtime ADR        |
| 技术层 ADR       | ADR-300~399，技术选型和实现细节，可替换升级                           | Technical ADR      |
| 架构委员会         | 宪法层 ADR 唯一审批主体，需全体一致同意                                | Architecture Board |

### 架构模式术语

| 术语               | 定义                                            | 英文对照                |
|------------------|-----------------------------------------------|---------------------|
| 模块化单体            | 单进程部署，按业务能力划分独立模块，物理分离，逻辑松耦合                  | Modular Monolith    |
| 垂直切片             | 以单个业务用例为最小代码/目录/依赖单元，横穿端到端实现                  | Vertical Slice      |
| Use Case         | 端到端业务用例，是业务功能的最小单元                            | Use Case            |
| Handler          | 业务用例的唯一决策实现，拥有全部业务决策权                         | Handler             |
| Command          | 代表"写操作"的单一职责消息，修改系统状态                         | Command             |
| Query            | 代表"读操作"的单一职责消息，不修改系统状态                        | Query               |
| CQRS             | Command-Query Responsibility Segregation，命令-查询职责分离 | CQRS                |
| 契约（Contract）     | 只读、单向、版本化的数据 DTO，只用于模块间信息传递，不含业务决策           | Contract            |
| 领域事件             | 描述已发生的业务事实，用于模块内或模块间异步通信                      | Domain Event        |
| 领域模型/领域实体        | 业务内聚的复杂类型，封装业务不变量，不允许跨模块共享                   | Domain Model/Entity |

### 技术实现术语

| 术语             | 定义                                      | 英文对照               |
|----------------|-----------------------------------------|---------------------|
| Platform 层     | 技术能力专属层，提供基础设施能力，不可感知业务                 | Platform Layer      |
| Application 层  | 应用层，负责模块装配、生命周期管理                       | Application Layer   |
| Host 层         | 宿主层，负责 HTTP/gRPC 等协议适配，不包含业务逻辑           | Host Layer          |
| Modules        | 业务模块层，按业务能力划分的独立模块                      | Modules             |
| Endpoint       | HTTP/gRPC 等协议的入口点，仅做请求适配，不含业务逻辑         | Endpoint            |
| CPM            | Central Package Management，中央包管理，统一管理依赖版本 | CPM                 |

### 测试与治理术语

| 术语         | 定义                                             | 英文对照                    |
|------------|------------------------------------------------|-----------------------|
| 架构测试       | 可自动执行的结构约束型测试，验证 ADR 规则                       | Architecture Test     |
| L1 测试      | 静态可执行自动化测试（如 NetArchTest）                       | Level 1 Test          |
| L2 测试      | 语义半自动化测试（如 Roslyn Analyzer、启发式检查）             | Level 2 Test          |
| L3 测试      | 人工 Gate，需要人工审查判断                               | Level 3 Test          |

---

---

## Decision（裁决）

### 编号分层规则（ADR-0006.1）

**规则**：
- 编号段直接对应架构层级：
  - `ADR-0001~0009`：宪法层，系统根基
  - `ADR-0000, 900~999`：治理层，流程管理
  - `ADR-100~199`：结构层，静态结构细化
  - `ADR-200~299`：运行层，运行时行为细化
  - `ADR-300~399`：技术层，技术选型

**判定**：
- ❌ 编号段与内容层级不匹配（如治理内容用 100~399）
- ❌ 技术选型用 900~999
- ✅ 编号段正确对应架构层级

### 编号格式规则（ADR-0006.2）

**规则**：
- 标准格式：`ADR-XXXX`
- 文件命名：`ADR-XXXX-descriptive-title.md`
- 文件名必须与 ADR 编号一致

**判定**：
- ❌ 文件名格式不符合规范（如使用下划线或驼峰命名）
- ❌ 文件名与内部 ADR 编号不一致
- ✅ 格式与编号完全符合规范

### 前导零规则（ADR-0006.3）

**规则**：
- 宪法层（0001~0009）**必须**使用前导零
- 编号格式为 `ADR-000X`（X 为 1-9）

**判定**：
- ❌ 宪法层 ADR 不使用前导零（如 ADR-1）
- ✅ 宪法层正确使用前导零（如 ADR-0001）

### 前导零禁用规则（ADR-0006.4）

**规则**：
- 非宪法层（100+）**不应**使用多余前导零
- 特殊例外：ADR-0000 和 ADR-900 允许使用前导零

**判定**：
- ❌ 非宪法层使用多余前导零（如 ADR-0120）
- ✅ 非宪法层不使用前导零（如 ADR-120）

### 目录归属规则（ADR-0006.5）

**规则**：
- ADR 文件必须位于与其编号段对应的正确目录
- 编号段与目录映射关系：
  - 0001~0009 → constitutional/
  - 0000, 900~999 → governance/
  - 100~199 → structure/
  - 200~299 → runtime/
  - 300~399 → technical/

**判定**：
- ❌ ADR 文件位于错误目录（如 ADR-120 在 governance/）
- ✅ ADR 文件位于正确目录

### 特殊编号（ADR-0006.6）

**规则**：
- ADR-0000：治理层，架构测试元规则，保持 0000 编号不变
- ADR-0006：宪法层，术语与编号元规则
- ADR-0007~0009：预留，非经架构委员会批准不得启用

**判定**：
- ❌ 未授权使用 0007~0009
- ❌ ADR-0000 添加非元机制内容
- ✅ 特殊编号按规则使用

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0006_Architecture_Tests.cs` 强制验证：

- ADR-0006.1: 编号段与层级匹配检查
- ADR-0006.2: 文件名与编号一致性检查
- ADR-0006.3: 宪法层前导零使用检查
- ADR-0006.4: 非宪法层前导零禁用检查
- ADR-0006.5: ADR 文件目录归属检查
- ADR-0006.6: 术语使用一致性审查（人工）

**有一项违规视为架构违规，CI 自动阻断。**

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

**被依赖（Depended By）**：
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md) - ADR 流程依赖编号规则
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-124：Endpoint 命名及参数约束规范](../structure/ADR-124-endpoint-naming-constraints.md)
- [ADR-120：领域事件命名规范](../structure/ADR-120-domain-event-naming-convention.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)
- [ADR-360：CI/CD Pipeline 流程标准化](../technical/ADR-360-cicd-pipeline-standardization.md)
- [ADR-340：结构化日志与监控约束](../technical/ADR-340-structured-logging-monitoring-constraints.md)
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-910：README 编写与维护宪法](../governance/ADR-910-readme-governance-constitution.md)
- 所有 ADR - 编号规范适用于所有 ADR

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - 术语定义参考核心架构（注：避免循环依赖，使用相关关系）
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 层级术语
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 命名规范
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - CQRS 术语
- [ADR-0008：文档编写与维护宪法](./ADR-0008-documentation-governance-constitution.md) - 文档术语

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
