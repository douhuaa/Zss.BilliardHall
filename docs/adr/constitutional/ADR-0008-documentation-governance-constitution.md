---
adr: ADR-0008
title: "文档编写与维护宪法"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-0008：文档编写与维护宪法

---
> ⚖️ **本 ADR 是所有文档的元规则，定义文档分级、边界与裁决权力的唯一裁决源。**


---

## 聚焦内容（Focus）

- 文档分级与唯一裁决权划分
- 各类文档允许表达的内容边界
- 非 ADR 文档的强制约束
- ADR 必需章节与禁用语言
- 文档变更治理与冲突裁决

---

## 术语表（Glossary）

| 术语         | 定义                                              | 英文对照                      |
|------------|------------------------------------------------|---------------------------|
| 裁决力        | 在架构冲突时能作为最终判定依据的权力                             | Decision Authority        |
| 宪法级文档      | 拥有最高裁决力的文档，即 ADR                               | Constitutional Document   |
| 治理级文档      | 定义行为规范和流程的文档，如 Instructions/Agents             | Governance Document       |
| 执行级文档      | 输出事实的文档，如 Skills                                | Execution Document        |
| 说明级文档      | 无裁决力的辅助文档，如 README/Guide                        | Descriptive Document      |
| 语义扩权       | 非 ADR 文档试图定义、修改或解释架构规则                          | Semantic Privilege Escalation |
| 文档越界       | 文档在其权限边界外定义规则或做出裁决                             | Document Boundary Violation |

---

## 决策（Decision）

### 文档分级与裁决权（ADR-0008.1）

**规则**：

| 级别  | 类型                    | 裁决力   | 示例                     | 权限边界                |
|-----|------------------------|-------|------------------------|--------------------|
| 宪法级 | ADR                    | **最高** | ADR-0001 ~ ADR-0099    | 定义规则、明确约束、裁决冲突    |
| 治理级 | Instructions / Agents  | 中     | copilot-instructions.md | 定义流程、行为规范、不得覆盖 ADR |
| 执行级 | Skills                 | 低（事实） | scan-cross-module-refs | 仅输出事实、不得解释或判断     |
| 说明级 | README / Guide         | **无**  | docs/*.md              | 仅解释使用方法、必须声明无裁决力  |

**核心原则**：

> **只有 ADR 具备裁决力。任何非 ADR 文档，即使全文逐字引用 ADR，也不具备裁决力。**

**判定**：
- ❌ 非 ADR 文档定义架构规则
- ❌ 非 ADR 文档试图裁决架构冲突
- ❌ 非 ADR 文档修改或解释 ADR 语义
- ✅ ADR 是唯一裁决源

### ADR 允许与禁止的内容（ADR-0008.2）

**ADR 仅允许**：
- ✅ 定义规则、术语、裁决逻辑
- ✅ 明确"允许 / 禁止 / 例外"
- ✅ 定义违规处理方式
- ✅ 定义执行机制（测试、CI、人工审查）

**ADR 禁止包含**：
- ❌ 示例代码实现细节（移至 README/Guide）
- ❌ 操作步骤、How-to 指南（移至 Guide）
- ❌ 工具参数、命令行细节（移至 Guide）
- ❌ 教学示例和场景说明（移至 Prompts）

**判定**：
- ❌ ADR 包含详细实施示例
- ❌ ADR 包含工具使用说明
- ✅ ADR 仅定义规则和裁决逻辑

### 非 ADR 文档强制约束（ADR-0008.3）

**Instructions/Agents 必须**：
- ✅ 显式声明所服从的 ADR 编号
- ✅ 声明"冲突时以 ADR 正文为准"
- ❌ 禁止引入新规则或术语
- ❌ 禁止覆盖或弱化 ADR

**Skills 必须**：
- ✅ 只输出事实，不输出判断
- ❌ 禁止解释 ADR
- ❌ 禁止输出"合规/不合规"结论
- ❌ 禁止包含"建议"或"推荐"

**README/Guide 必须**：
- ✅ 只解释"如何使用"
- ✅ 首部声明"无裁决力"
- ❌ 禁止定义规则或做架构判断

> **注意**：README 详细约束见 [ADR-910](../../governance/ADR-910-readme-governance-constitution.md)

**判定**：
- ❌ Instructions 定义新架构规则
- ❌ Skills 输出"违反 ADR-0001"
- ❌ README 包含"模块必须..."
- ✅ 文档遵守权限边界

### ADR 必需章节（ADR-0008.4）

**规则**：

所有 ADR **必须**包含：
- ✅ 状态（Proposed/Adopted/Final/Superseded）
- ✅ 级别（宪法层/结构层/运行层/技术层/治理层）
- ✅ 聚焦内容（Focus）
- ✅ 术语表（Glossary）- 如有新术语
- ✅ 核心决策（Decision）
- ✅ 规则本体（Rule）- 详细约束
- ✅ 执法模型（Enforcement）- 如需自动化测试

**判定**：
- ❌ ADR 缺少必需章节
- ❌ ADR 章节顺序错乱
- ✅ ADR 结构完整符合标准

### ADR 禁用语言（ADR-0008.5）

**规则**：

ADR 中**禁止**使用模糊词汇：
- ❌ "建议"（suggest）
- ❌ "推荐"（recommend）
- ❌ "通常"（usually）
- ❌ "可能"（might）
- ❌ "尽量"（try to）

**只允许裁决性语言**：
- ✅ "必须"（MUST）
- ✅ "禁止"（MUST NOT）
- ✅ "应当"（SHALL）
- ✅ "允许"（MAY）

**核心原则**：

> **ADR 中没有"建议"，只有"规则"。**

**判定**：
- ❌ ADR 包含"建议使用..."
- ❌ ADR 包含"通常情况下..."
- ✅ ADR 仅使用裁决性语言

### 文档变更治理（ADR-0008.6）

**规则**：

| 文档类型                | 变更要求           | 审批流程       |
|---------------------|----------------|------------|
| ADR（宪法层）            | 架构修宪，需全体一致同意   | 架构委员会全票通过  |
| ADR（其他层）            | 需架构评审          | Tech Lead  |
| Instructions/Agents | 若影响规则理解，需回溯 ADR | Tech Lead  |
| Skills              | 若改变输出语义，需评审    | Code Review |
| README/Guide        | 自由修改，但需保持声明    | Code Review |

**冲突裁决优先级**：

```
ADR 正文 > Instructions > Agents > Skills > Prompts > README/Guide
```

**判定**：
- ❌ 宪法层 ADR 未经架构委员会修改
- ❌ 以"辅导材料说了"为由违反 ADR
- ❌ 仅更新辅导材料不修订 ADR
- ✅ 文档变更按分级权限审批

### 违规处理（ADR-0008.7）

**规则**：

| 违规行为                    | 处理方式      | 示例                         |
|-------------------------|-----------|----------------------------|
| 非 ADR 文档引入规则            | PR **必须拒绝** | README 定义"模块必须使用事件通信"     |
| README 试图裁决架构问题         | 标记为违规     | README 判定"这个设计不符合架构"       |
| Skills 输出判断性结论          | Agent 阻断   | Skills 输出"违反 ADR-0001"    |
| Instructions 覆盖 ADR 语义 | PR 必须拒绝   | Instructions 弱化"禁止"为"不推荐" |
| 文档缺失"无裁决力"声明           | CI 失败      | README 未声明无裁决力             |

**判定**：
- ❌ 检测到上述任何违规行为
- ✅ 文档遵守所有约束

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 文档术语遵循统一规范

**被依赖（Depended By）**：
- [ADR-910：README 编写与维护](../governance/ADR-910-readme-governance-constitution.md) - README 约束基于本 ADR
- [ADR-920：示例治理](../governance/ADR-920-examples-governance-constitution.md) - 示例文档约束基于本 ADR
- [ADR-940：ADR 关系与溯源管理](../governance/ADR-940-adr-relationship-traceability-management.md) - 关系声明是文档的一部分
- [ADR-946：ADR 标题级别即语义级别约束](../governance/ADR-946-adr-heading-level-semantic-constraint.md) - ADR 文档结构规范
- [ADR-947：关系声明区的结构与解析安全规则](../governance/ADR-947-relationship-section-structure-parsing-safety.md) - 文档结构规范
- [ADR-950：指南与 FAQ 文档治理](../governance/ADR-950-guide-faq-documentation-governance.md) - 非裁决性文档约束基于本 ADR
- [ADR-955：文档搜索与可发现性](../governance/ADR-955-documentation-search-discoverability.md) - 文档组织规范基于本 ADR
- [ADR-960：新人入职文档治理](../governance/ADR-960-onboarding-documentation-governance.md) - 入职文档约束基于本 ADR
- [ADR-965：交互式学习路径](../governance/ADR-965-onboarding-interactive-learning-path.md) - 学习文档约束基于本 ADR
- [ADR-970：自动化工具日志集成标准](../governance/ADR-970-automation-log-integration-standard.md) - 日志文档约束基于本 ADR
- [ADR-975：文档质量监控](../governance/ADR-975-documentation-quality-monitoring.md) - 质量监控基于本 ADR
- [ADR-990：文档演进路线图](../governance/ADR-990-documentation-evolution-roadmap.md) - 演进规划基于本 ADR
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md)
- [ADR-930：代码审查与 ADR 合规自检流程](../governance/ADR-930-code-review-compliance.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0007：Agent 行为与权限宪法](./ADR-0007-agent-behavior-permissions-constitution.md) - Agent 行为与文档权威相互关联
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-adr-process.md) - ADR 流程与文档治理相关

---

## 快速参考表

| 约束编号       | 约束描述              | 测试方式            | 测试用例                                  | 必须遵守 |
|------------|-------------------|-----------------|---------------------------------------|------|
| ADR-0008.1 | 只有 ADR 具备裁决力      | L3 - 人工审查       | Documentation_Authority_Check         | ✅    |
| ADR-0008.2 | ADR 内容边界          | L3 - 人工审查       | ADR_Content_Boundary_Check            | ✅    |
| ADR-0008.3 | 非 ADR 文档权限边界      | L2 - 脚本检查+人工审查  | Non_ADR_Document_Boundary_Check       | ✅    |
| ADR-0008.4 | ADR 必需章节          | L1 - 脚本检查       | ADR_Required_Sections_Check           | ✅    |
| ADR-0008.5 | ADR 禁用语言          | L2 - 脚本检查禁用词    | ADR_Prohibited_Language_Check         | ✅    |
| ADR-0008.6 | 文档变更治理            | L3 - 人工审查       | Documentation_Change_Governance_Check | ✅    |
| ADR-0008.7 | 违规处理              | L2 - 脚本检查+人工审查  | Documentation_Violation_Check         | ✅    |

> **级别说明**：L1=静态自动化（脚本检查），L2=语义半自动（脚本+启发式），L3=人工Gate

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0008_Architecture_Tests.cs` 强制验证：

- ADR 必需章节检查（Status/Level/Focus/Decision/Enforcement）
- ADR 禁用语言检查（suggest/recommend/usually/might）
- 非 ADR 文档权威声明检查
- Skills 输出判断性词汇检查
- README/Guide 裁决性语言检查
- 文档变更版本历史记录检查

**CI 强制检查**：
- 所有非 ADR 文档是否包含权威依据或无裁决力声明
- Skills 输出是否包含判断性词汇
- README/Guide 是否使用裁决性语言

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] ADR 是否包含所有必需章节？
- [ ] ADR 是否只使用裁决性语言（必须/禁止/允许）？
- [ ] 非 ADR 文档是否声明所服从的 ADR？
- [ ] README/Guide 是否声明"无裁决力"？
- [ ] Skills 输出是否只有事实，无判断？
- [ ] 文档变更是否按分级权限审批？
- [ ] 是否更新了版本历史？

---

## 版本历史

| 版本  | 日期         | 变更说明                                  |
|-----|------------|---------------------------------------|
| 2.0 | 2026-01-26 | 裁决型重构，移除冗余，聚焦核心规则                   |
| 1.1 | 2026-01-25 | README 约束独立为 ADR-910，本 ADR 仅保留核心原则  |
| 1.0 | 2026-01-25 | 初版，定义文档治理                             |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、文档边界案例、变更流程详解）请查阅：
- [ADR-0008 Copilot Prompts](../../copilot/adr-0008.prompts.md)
- [ADR-910: README 编写与维护宪法](../../governance/ADR-910-readme-governance-constitution.md)
- `docs/templates/adr-template.md`：ADR 编写模板
