---
adr: ADR-006
title: "术语与编号宪法"
status: Final
level: Constitutional
version: "2.0"
deciders: "Architecture Board"
date: 2026-02-04
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-006：术语与编号宪法

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
| 宪法层 ADR       | ADR-####~####，系统根基，不可推翻，只能细化，破例需特批                  | Constitutional ADR |
| 治理层 ADR       | ADR-####~####，架构过程、测试、CI、审批、破例管理 | Governance ADR     |
| 结构层 ADR       | ADR-####~####，对宪法层结构约束的细化和补充（命名、组织、静态边界）              | Structure ADR      |
| 运行层 ADR       | ADR-####~####，对宪法层运行时约束的细化和补充（生命周期、事件、异常）             | Runtime ADR        |
| 技术层 ADR       | ADR-####~####，技术选型和实现细节，可替换升级                           | Technical ADR      |
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

### "宪法"术语的语境区分

> **重要说明**："宪法"一词在本架构体系中有明确的使用边界。

**宪法层 ADR（Constitutional ADR）**：
- 编号范围：ADR-####~ADR-####
- 位置：`docs/adr/constitutional/` 目录
- 含义：定义系统的**根本架构约束**（代码结构、模块隔离、命名空间等）
- 示例：
  - ADR-####：模块化单体与垂直切片架构
  - ADR-006：术语与编号宪法
  - ADR-####：Agent 行为与权限宪法
  - ADR-####：文档编写与维护宪法

**治理层 ADR（Governance ADR）**：
- 编号范围：ADR-####~ADR-####
- 位置：`docs/adr/governance/` 目录
- 含义：定义**治理流程和元规则**（ADR 流程、测试规范、文档规范等）
- 术语使用规范：
  - ✅ 使用"治理规范"（Governance Standard）
  - ✅ 使用"元规则"（Meta-Rule）
  - ❌ 避免使用"宪法"以免与 Constitutional 层混淆
- 示例：
  - ADR-####：架构测试与 CI 治理元规则
  - ADR-####：语义元规则
  - ADR-####：README 编写与维护治理规范
  - ADR-####：示例代码治理规范

**核心原则**：
- "宪法"一词**专属于 Constitutional 层**（约束代码架构）
- Governance 层使用"治理规范"或"元规则"（约束治理行为）
- 这种区分确保术语清晰，降低认知负担

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-006 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-006_<Rule>_<Clause>
> ```

---

### ADR-006_1：编号分层规则（Rule）

#### ADR-006_1_1 编号段层级映射

编号段直接对应架构层级：
- `ADR-####~####`：宪法层，系统根基
- `ADR-####~####`：治理层，流程管理
- `ADR-####~####`：结构层，静态结构细化
- `ADR-####~####`：运行层，运行时行为细化
- `ADR-####~####`：技术层，技术选型

**判定**：
- ❌ 编号段与内容层级不匹配（如治理内容用 100~399）
- ❌ 技术选型用 900~999
- ✅ 编号段正确对应架构层级

---

### ADR-006_2：编号格式规则（Rule）

#### ADR-006_2_1 标准编号格式

标准格式：`ADR-XXX`（3位数字，000-999）

**判定**：
- ❌ 使用非3位格式（如 ADR-1, ADR-####, ADR-####0）
- ✅ 使用正确的3位格式

#### ADR-006_2_2 文件命名规范

- 文件命名：`ADR-XXX-descriptive-title.md`
- 文件名必须与 ADR 编号一致
- 标题使用小写字母和连字符

**判定**：
- ❌ 文件名格式不符合规范（如使用下划线或驼峰命名）
- ❌ 文件名与内部 ADR 编号不一致
- ✅ 格式与编号完全符合规范

---

### ADR-006_3：前导零规则（Rule）

#### ADR-006_3_1 前导零强制要求

- 所有 ADR 编号统一使用3位格式（000-999）
- 编号小于100时**必须**使用前导零
  - 宪法层：`ADR-####` 到 `ADR-####`
  - 其他小于100的编号：`ADR-####` 到 `ADR-####`
- 编号大于等于100时自然为3位数字
  - 结构层：`ADR-####` 到 `ADR-####`
  - 运行层：`ADR-####` 到 `ADR-####`
  - 技术层：`ADR-####` 到 `ADR-####`
  - 治理层：`ADR-####` 到 `ADR-####`

**判定**：
- ❌ 小于100的编号不使用前导零（如 ADR-1, ADR-10）
- ✅ 所有编号正确使用3位格式

---

### ADR-006_4：目录归属规则（Rule）

#### ADR-006_4_1 目录层级映射

ADR 文件必须位于与其编号段对应的正确目录：
- 001~009 → constitutional/
- 900~999 → governance/
- 100~199 → structure/
- 200~299 → runtime/
- 300~399 → technical/

**判定**：
- ❌ ADR 文件位于错误目录（如 ADR-#### 在 governance/）
- ✅ ADR 文件位于正确目录

---

### ADR-006_5：特殊编号规则（Rule）

#### ADR-006_5_1 元规则编号保护

- ADR-####：治理层，架构测试元规则
- ADR-006：宪法层，术语与编号元规则
- ADR-####~####：预留，非经架构委员会批准不得启用

**判定**：
- ❌ 未授权使用 007~009
- ❌ ADR-#### 添加非元机制内容
- ✅ 特殊编号按规则使用

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-006 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR_006/` 目录下的测试类强制验证：
> - `ADR_006_1_Architecture_Tests.cs` - Rule 1（编号分层规则）
> - `ADR_006_2_Architecture_Tests.cs` - Rule 2（编号格式规则）
> - `ADR_006_3_Architecture_Tests.cs` - Rule 3（前导零规则）
> - `ADR_006_4_Architecture_Tests.cs` - Rule 4（目录归属规则）
> - `ADR_006_5_Architecture_Tests.cs` - Rule 5（特殊编号规则）

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-006_1_1** | L1 | ArchitectureTests 验证编号段与层级映射 | §ADR-006_1_1 |
| **ADR-006_2_1** | L1 | ArchitectureTests 验证3位编号格式 | §ADR-006_2_1 |
| **ADR-006_2_2** | L1 | ArchitectureTests 验证文件命名规范 | §ADR-006_2_2 |
| **ADR-006_3_1** | L1 | ArchitectureTests 验证前导零使用 | §ADR-006_3_1 |
| **ADR-006_4_1** | L1 | ArchitectureTests 验证目录归属 | §ADR-006_4_1 |
| **ADR-006_5_1** | L3 | 人工审查特殊编号使用 | §ADR-006_5_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

**有一项 L1 违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **自然语言写作风格规范**：不规定文档的语气、修辞手法或写作技巧
- **编程语言命名规范**：不涉及代码变量、函数、类的命名约定
- **版本号语义规则**：不定义 Semver 或其他版本号格式的具体规则
- **翻译和多语言术语对照**：不建立术语的多语言翻译映射表
- **术语的历史溯源**：不追溯技术术语的来源和演变历史
- **行业标准术语的引入时机**：不规定何时采用新兴技术术语
- **缩写词的读音规范**：不定义缩写词应如何发音
- **术语的过时淘汰流程**：不建立术语废弃和替换的流程机制

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 编号规则违反

- ❌ **禁止使用非标准编号格式**：如 `ADR_001`、`adr-1`、`ADR-A-001`、`ADR-####`（4位），正确格式为 `ADR-####`（3位数字）
- ❌ **禁止跳号或重复编号**：必须按顺序递增（001, 002, 003...）
- ❌ **禁止私自修改已发布 ADR 的编号**：保持编号不变，使用"已废弃"状态

### 术语使用违反

- ❌ **禁止在同一文档中混用同义术语**：统一使用术语表中的标准术语
- ❌ **禁止创造未经审批的新术语**：使用标准术语或提交新术语申请
- ❌ **禁止使用模糊不清的指代**：明确引用具体的 ADR 或概念

### 一致性违反

- ❌ **禁止在不同文档中对同一概念使用不同术语**：保持术语一致性
- ❌ **禁止缩写词不加注释直接使用**：首次出现时注明全称
- ❌ **禁止使用未定义的专有名词**：添加必要的解释说明

### 文档引用违反

- ❌ **禁止使用模糊的文档引用**：使用明确的文件路径和链接
- ❌ **禁止使用失效的编号引用**：引用前验证文档存在性
- ❌ **禁止循环引用形成死锁**：梳理依赖关系，必要时拆分文档


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900

**被依赖（Depended By）**：
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md) - ADR 流程依赖编号规则
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-124：Endpoint 命名及参数约束规范](../structure/ADR-124-endpoint-naming-constraints.md)
- [ADR-120：领域事件命名规范](../structure/ADR-120-domain-event-naming-convention.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)
- [ADR-360：CI/CD Pipeline 流程标准化](../technical/ADR-360-cicd-pipeline-standardization.md)
- [ADR-340：结构化日志与监控约束](../technical/ADR-340-structured-logging-monitoring-constraints.md)
- [ADR-007：Agent 行为与权限宪法](../constitutional/ADR-007-agent-behavior-permissions-constitution.md)
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)
- [ADR-910：README 编写与维护治理规范](../governance/ADR-910-readme-governance-constitution.md)
- 所有 ADR - 编号规范适用于所有 ADR

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - 术语定义参考核心架构（注：避免循环依赖，使用相关关系）
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 层级术语
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 命名规范
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - CQRS 术语
- [ADR-008：文档编写与维护宪法](./ADR-008-documentation-governance-constitution.md) - 文档术语

---

---

## References（非裁决性参考）


**官方标准**：
- [Semantic Versioning 2.0.0](https://semver.org/) - 语义化版本规范
- [RFC 2119: Key words for use in RFCs](https://www.ietf.org/rfc/rfc2119.txt) - 规范性语言标准（MUST/SHOULD/MAY）
- [IEEE Standard Glossary of Software Engineering Terminology](https://ieeexplore.ieee.org/document/159342) - IEEE 软件工程术语标准

**写作指南**：
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/) - 技术写作风格指南
- [Google Developer Documentation Style Guide](https://developers.google.com/style) - 术语一致性最佳实践
- [ADR GitHub Organization](https://adr.github.io/) - ADR 社区标准

**相关内部文档**：
- [ADR-008：文档编写与维护宪法](./ADR-008-documentation-governance-constitution.md) - 文档风格规范
- [ADR-902：ADR 模板结构契约](../governance/ADR-902-adr-template-structure-contract.md) - ADR 结构规范


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 2.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系，将 5 条规则（编号分层、编号格式、前导零、目录归属、特殊编号）重构为 5 个 Rule（Rule 1-5），共 6 个 Clause。测试拆分为 5 个独立测试类文件。 | Architecture Board |
| 1.1 | 2026-01-30 | 更新版本号，细化规则说明 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
