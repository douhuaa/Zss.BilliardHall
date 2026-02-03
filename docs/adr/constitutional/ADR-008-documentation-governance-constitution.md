---
adr: ADR-008
title: "文档编写与维护宪法"
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


# ADR-008：文档编写与维护宪法

> ⚖️ **本 ADR 是所有文档的元规则，定义文档分级、边界与裁决权力的唯一裁决源。**

**状态**：✅ Final（裁决型ADR）  
## Focus（聚焦内容）

- 文档分级与唯一裁决权划分
- 各类文档允许表达的内容边界
- 非 ADR 文档的强制约束
- ADR 必需章节与禁用语言
- 文档变更治理与冲突裁决

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|------------------------------------------------|---------------------------|
| 裁决力        | 在架构冲突时能作为最终判定依据的权力                             | Decision Authority        |
| 宪法级文档      | 拥有最高裁决力的文档，即 ADR                               | Constitutional Document   |
| 治理级文档      | 定义行为规范和流程的文档，如 Instructions/Agents             | Governance Document       |
| 执行级文档      | 输出事实的文档，如 Skills                                | Execution Document        |
| 说明级文档      | 无裁决力的辅助文档，如 README/Guide                        | Descriptive Document      |
| 语义扩权       | 非 ADR 文档试图定义、修改或解释架构规则                          | Semantic Privilege Escalation |
| 文档越界       | 文档在其权限边界外定义规则或做出裁决                             | Document Boundary Violation |

---

---

## Decision（裁决）

### 文档分级与裁决权（ADR-008.1）

**规则**：

| 级别  | 类型                    | 裁决力   | 示例                     | 权限边界                |
|-----|------------------------|-------|------------------------|--------------------|
| 宪法级 | ADR                    | **最高** | ADR-001 ~ ADR-0099    | 定义规则、明确约束、裁决冲突    |
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

### ADR 允许与禁止的内容（ADR-008.2）

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

### 非 ADR 文档强制约束（ADR-008.3）

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
- ❌ Skills 输出"违反 ADR-001"
- ❌ README 包含"模块必须..."
- ✅ 文档遵守权限边界

### ADR 必需章节（ADR-008.4）

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

### ADR 禁用语言（ADR-008.5）

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

### 文档变更治理（ADR-008.6）

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

### 违规处理（ADR-008.7）

**规则**：

| 违规行为                    | 处理方式      | 示例                         |
|-------------------------|-----------|----------------------------|
| 非 ADR 文档引入规则            | PR **必须拒绝** | README 定义"模块必须使用事件通信"     |
| README 试图裁决架构问题         | 标记为违规     | README 判定"这个设计不符合架构"       |
| Skills 输出判断性结论          | Agent 阻断   | Skills 输出"违反 ADR-001"    |
| Instructions 覆盖 ADR 语义 | PR 必须拒绝   | Instructions 弱化"禁止"为"不推荐" |
| 文档缺失"无裁决力"声明           | CI 失败      | README 未声明无裁决力             |

**判定**：
- ❌ 检测到上述任何违规行为
- ✅ 文档遵守所有约束

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_008_Architecture_Tests.cs` 强制验证：

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
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **文档托管平台选择**：不规定使用 GitHub、GitLab 还是其他平台
- **文档渲染工具和主题**：不涉及 MkDocs、Docusaurus 等工具的配置
- **文档的 SEO 优化**：不涉及搜索引擎优化策略
- **文档的自动化翻译**：不建立多语言文档的翻译流程
- **文档的版权和许可证**：不定义文档的开源许可证类型
- **文档的阅读分析和统计**：不涉及文档访问量、阅读时长等指标
- **文档的评论和反馈系统**：不建立用户评论和反馈的技术实现
- **文档的打印和导出格式**：不规定 PDF、Word 等导出格式的样式

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 格式违规

- ❌ **禁止使用非简体中文撰写文档正文**：统一使用简体中文（代码、专有名词除外）
- ❌ **禁止使用营销语言和夸张修辞**：保持客观、专业的表述
- ❌ **禁止缺少代码语言标记**：代码块必须标记语言类型
- ❌ **禁止使用绝对路径链接**：使用相对路径链接

### 内容质量违规

- ❌ **禁止文档缺少核心章节**：ADR 必须包含状态、级别、决策等必需章节
- ❌ **禁止使用模糊不清的描述**：提供具体、可验证的描述
- ❌ **禁止文档内容自相矛盾**：保持逻辑一致性
- ❌ **禁止抄袭外部内容而不注明出处**：引用来源并注明链接

### 维护流程违规

- ❌ **禁止直接修改已批准的 ADR 决策部分**：创建新 ADR 替代或补充
- ❌ **禁止跳过文档审核流程**：经过至少一名审核者批准
- ❌ **禁止删除文档而不标记为废弃**：标记为 Deprecated 并说明原因

### 索引和链接违规

- ❌ **禁止新增文档后不更新索引**：同时更新相关索引文件
- ❌ **禁止使用失效的链接**：定期检查并修复失效链接
- ❌ **禁止缺少双向引用**：在相关文档中添加双向链接

### 可访问性违规

- ❌ **禁止图片缺少 alt 文本**：为所有图片添加描述性文本
- ❌ **禁止使用纯表情符号传递关键信息**：表情符号需配合文字说明
- ❌ **禁止表格过于复杂无法阅读**：拆分为多个表格或使用列表


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 文档术语遵循统一规范

**被依赖（Depended By）**：
- [ADR-902：ADR 结构与章节规范](../governance/ADR-902-adr-template-structure-contract.md) - ADR 结构规范基于文档治理
- [ADR-905：架构约束分类与裁决实施映射](../governance/ADR-905-constraint-classification.md) - 约束分类基于文档治理
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
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md)
- [ADR-930：代码审查与 ADR 合规自检流程](../governance/ADR-930-code-review-compliance.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-007：Agent 行为与权限宪法](./ADR-007-agent-behavior-permissions-constitution.md) - Agent 行为与文档权威相互关联，共同规范 Agent 输出与文档裁决力
- [ADR-900：ADR 新增与修订流程](../governance/ADR-900-architecture-tests.md) - ADR 流程与文档治理相关

---

---

## References（非裁决性参考）

**写作指南**：
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/) - 技术写作标准
- [Google Developer Documentation Style Guide](https://developers.google.com/style) - 文档风格指南
- [Write the Docs](https://www.writethedocs.org/) - 文档工程社区
- [Diátaxis Documentation Framework](https://diataxis.fr/) - 文档架构方法论
- [Markdown Guide](https://www.markdownguide.org/) - Markdown 语法规范
- [CommonMark Spec](https://spec.commonmark.org/) - Markdown 标准规范
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/) - Web 内容可访问性指南

**相关内部文档**：
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 术语一致性规范
- [ADR-007：Agent 行为与权限宪法](./ADR-007-agent-behavior-permissions-constitution.md) - Agent 文档生成规范
- [ADR-902：ADR 模板结构契约](../governance/ADR-902-adr-template-structure-contract.md) - ADR 结构规范


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
