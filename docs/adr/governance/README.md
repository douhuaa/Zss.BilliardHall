# 治理层 ADR（ADR-0000, ADR-900~999）

> ⚠️ **无裁决力声明**：本文档无架构裁决权，所有决策以 ADR 正文为准。

---

## 概述

治理层 ADR 定义系统的**流程、变更、审查、文档和示例管理规则**。ADR-0000 作为**元决策源**，确立所有治理规则的裁决力基础。

### 核心原则

1. **ADR-0000 是唯一元决策源**：所有治理规则的裁决力来自 ADR-0000
2. **测试一一映射与自动阻断**：架构约束须有对应测试，违规自动阻断
3. **破例治理闭环**：所有破例须记录、审批、归还，过期自动失效
4. **三位一体交付**：ADR 变更须同步更新文档/测试/Prompts

---

## 治理体系分类

### 元治理（Meta-Governance）

- [ADR-0000：架构测试与 CI 治理元规则](ADR-0000-architecture-tests.md) ⭐
  - **地位**：元决策源，所有治理规则的裁决力基础
  - **核心**：测试映射、CI 阻断、破例管理、执行分级
  - **强制**：所有 ADR 须遵循本 ADR 的测试和 CI 机制

### 流程治理（Process Governance）

- [ADR-900：ADR 新增与修订流程](ADR-900-adr-process.md)
  - 定义 ADR 生命周期：新增、修订、废弃、权限、三位一体交付
- [ADR-930：代码审查与 ADR 合规自检流程](ADR-930-code-review-compliance.md)
  - 定义 PR 必填信息、Copilot 自检、架构测试失败处理、破例标注

### 文档治理（Documentation Governance）

- [ADR-910：README 编写与维护治理规范](ADR-910-readme-governance-constitution.md)
  - README 边界：说明性文档，无裁决性语言
- [ADR-920：示例代码治理规范](ADR-920-examples-governance-constitution.md)
  - 示例代码边界：演示用法，不得违反架构约束
- [ADR-950：Guide/FAQ 文档治理](ADR-950-guide-faq-documentation-governance.md)
  - 指南和 FAQ 的分离边界和权威声明
- [ADR-951：案例库管理](ADR-951-case-repository-management.md)
  - 案例代码的组织和维护规范
- [ADR-952：工程标准与 ADR 分离边界](ADR-952-engineering-standard-adr-boundary.md)
  - 工程标准与架构决策的清晰分界
- [ADR-955：文档搜索与可发现性](ADR-955-documentation-search-discoverability.md)
  - 文档索引、搜索和导航机制
- [ADR-960：新人入职文档治理](ADR-960-onboarding-documentation-governance.md)
  - 入职文档的组织和维护标准
- [ADR-965：入职互动学习路径](ADR-965-onboarding-interactive-learning-path.md)
  - 互动式入职学习体验设计

### ADR 关系与演进治理（ADR Relationship Governance）

- [ADR-940：ADR 关系与溯源管理治理规范](ADR-940-adr-relationship-traceability-management.md)
  - ADR 关系类型、双向一致性、关系图生成
- [ADR-945：ADR 时间线与演进视图](ADR-945-adr-timeline-evolution-view.md)
  - ADR 演进历史的可视化和溯源
- [ADR-946：ADR 标题层级语义约束](ADR-946-adr-heading-level-semantic-constraint.md)
  - ADR 文档结构的标准化约束
- [ADR-947：关系章节结构解析安全](ADR-947-relationship-section-structure-parsing-safety.md)
  - 关系声明的解析和验证规则
- [ADR-980：ADR 生命周期一体化同步机制](ADR-980-adr-lifecycle-synchronization.md)
  - ADR 变更的全链同步和一致性保障
- [ADR-990：文档演进路线图](ADR-990-documentation-evolution-roadmap.md)
  - 文档体系的长期演进规划

### 质量与监控治理（Quality Governance）

- [ADR-905：执行级别分类](ADR-905-enforcement-level-classification.md)
  - L1 静态/L2 语义/L3 人工的执行分级标准
- [ADR-907：ArchitectureTests 执法治理体系](ADR-907-architecture-tests-enforcement-governance.md) ⭐
  - 整合 ArchitectureTests 命名、组织、最小断言及 CI/Analyzer 映射规则
  - 实现完整的自动裁决闭环
- [ADR-907-A：ADR-907 对齐执行标准](ADR-907-A-adr-alignment-execution-standard.md) ⭐
  - **ADR-907 的官方执行附录**
  - 定义 ADR 向 Rule/Clause 双层编号体系对齐的强制规范
  - 包含权威性声明、对齐失败策略、测试绑定规则
- [ADR-970：自动化工具日志集成标准](ADR-970-automation-log-integration-standard.md)
  - CI/测试/Copilot 日志的统一格式和集成
- [ADR-975：文档质量监控](ADR-975-documentation-quality-monitoring.md)
  - 文档健康度指标和持续改进机制

---

## 合规与闭环机制

### 破例治理（Exception Management）

根据 **ADR-0000.Y 破例成本管理**，所有架构破例须：

- ✅ **强制字段**：到期版本号、偿还负责人、偿还计划、审批人
- ✅ **自动监控**：CI 定期扫描 `arch-violations.md`，过期即失败构建
- ✅ **延期限制**：最多延期 2 次，需重新审批
- ✅ **强制归还**：超过 2 次延期须强制偿还

**参考**：
- [arch-violations.md](/docs/summaries/arch-violations.md) - 破例记录表
- [ADR-0000.Y](/docs/adr/governance/ADR-0000-architecture-tests.md#adr-0000y破例须绑定偿还计划与到期监控) - 破例管理机制

### PR 与代码审查强制要求（ADR-930）

所有 PR 须：

- ✅ 填写变更类型和影响范围
- ✅ ADR 相关 PR 须通过 Copilot 自检
- ✅ 架构测试失败须说明原因和计划
- ✅ 至少一名责任人审查
- ✅ 破例须在 PR 和代码中明确标注

### README/示例治理（ADR-910/920）

- ✅ **README 禁止**：定义架构规则、使用裁决性语言（除非明确引用 ADR）
- ✅ **示例代码禁止**：违反架构约束、引入 ADR 未允许的模式
- ✅ **强制声明**：须包含"无裁决力声明"或"示例免责声明"

---

## 变更与演进机制

### 治理 ADR 变更权限（ADR-900）

| 层级/类型  | 新增权限           | 修订权限           | 公示期 | 审批要求  |
|--------|----------------|----------------|-----|-------|
| 元治理    | 架构委员会          | 架构委员会全体        | 2 周 | 全体一致  |
| 治理层    | Tech Lead/架构师  | Tech Lead/架构师  | 1 周 | 多数同意  |
| 其他     | Tech Lead/架构师  | Tech Lead/架构师  | 无   | 单人批准  |

### 三位一体交付要求（ADR-900）

所有 ADR 变更须同步更新：

1. **ADR 文档**：完整的 Focus/术语/决策/关系/版本历史
2. **架构测试**：对应的自动化测试用例
3. **Copilot Prompts**：场景化的提示词和示例

**不可**：只提交 ADR 文档，测试和 Prompt 后补（视为无效 ADR）

---

## 前瞻性改进建议

基于当前治理体系，建议进一步强化：

### 1. 提高 arch-violations.md 使用率

- ✅ 已实施 CI 定期扫描（每月第一天）
- ⏳ 建议：每周生成破例健康度报告
- ⏳ 建议：破例到期前 2 周提前预警

### 2. 强化责任人和归还计划

- ✅ arch-violations.md 已强制要求责任人和归还计划字段
- ⏳ 建议：所有治理规则表格增加"责任人"列
- ⏳ 建议：定期审计责任人执行情况

### 3. 定期治理和合规数据报告

- ⏳ 建议：季度架构健康度报告（测试失效率、破例量、CI 通过率）
- ⏳ 建议：治理 ADR 执行效果评估
- ⏳ 建议：团队治理成熟度评估

### 4. 治理变更同步索引机制

- ✅ 已要求 ADR 变更同步更新导航/映射表
- ⏳ 建议：自动化检测索引一致性
- ⏳ 建议：治理变更触发全链回归测试

---

## 快速参考

### 核心文档

| 文档                   | 用途              | 链接                                           |
|----------------------|-----------------|----------------------------------------------|
| ADR-0000             | 元决策源，测试和 CI 机制  | [查看](ADR-0000-architecture-tests.md)         |
| ADR-900              | ADR 生命周期流程     | [查看](ADR-900-adr-process.md)                 |
| ADR-907              | ArchitectureTests 执法治理体系 | [查看](ADR-907-architecture-tests-enforcement-governance.md) |
| ADR-907-A            | ADR-907 对齐执行标准（官方执行附录） | [查看](ADR-907-A-adr-alignment-execution-standard.md) |
| ADR-930              | PR 和代码审查流程     | [查看](ADR-930-code-review-compliance.md)      |
| arch-violations.md   | 破例记录表           | [查看](/docs/summaries/arch-violations.md)     |

### 治理工具

| 工具                             | 用途         | 链接                                                    |
|--------------------------------|------------|-------------------------------------------------------|
| architecture-tests.yml         | 架构测试 CI    | [查看](/.github/workflows/architecture-tests.yml)      |
| arch-violations-scanner.yml    | 破例扫描 CI    | [查看](/.github/workflows/arch-violations-scanner.yml) |
| adr-relationship-check.yml     | ADR 关系验证   | [查看](/.github/workflows/adr-relationship-check.yml)  |
| adr-version-sync.yml           | ADR 版本同步   | [查看](/.github/workflows/adr-version-sync.yml)        |

---

## 相关资源

- [宪法层 ADR](../constitutional/README.md) - 系统基础架构决策
- [结构层 ADR](../structure/README.md) - 模块和组件结构决策
- [运行层 ADR](../runtime/README.md) - 运行时行为决策
- [技术层 ADR](../technical/README.md) - 技术实现决策
- [Copilot 治理体系](/docs/copilot/README.md) - Copilot 提示词库
- [架构治理系统](/docs/ARCHITECTURE-GOVERNANCE-SYSTEM.md) - 完整治理体系

---

## 版本历史

| 版本  | 日期         | 变更说明                  | 修订人            |
|-----|------------|----------------------|----------------|
| 2.0 | 2026-01-26 | 强化治理机制、增加前瞻性改进建议     | GitHub Copilot |
| 1.0 | 2026-01-20 | 初版，基础治理 ADR 索引      | @douhuaa       |

---

**维护**：架构委员会  
**审核**：@douhuaa  
**状态**：✅ Active
