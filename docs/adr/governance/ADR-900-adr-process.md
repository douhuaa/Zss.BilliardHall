---
adr: ADR-900
title: "ADR 新增与修订流程"
status: Final
level: Governance
version: "1.2"
deciders: "Architecture Board"
date: 2026-01-27
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-900：ADR 新增与修订流程

> ⚖️ 本流程是 Zss.BilliardHall 唯一有效 ADR 生命周期闭环规范。任何未走本流程的 ADR 在本项目中视为无效，不具约束力。

**状态**：✅ Final（裁决型ADR）  
## Focus（聚焦内容）

- ADR 新增/修订/废弃全流程唯一标准
- 分层权限表与破例登记机制
- 三位一体（文档/测试/Prompt）交付与校验
- Copilot + 人工审查闭环
- 提交、审查、归档快速清单

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|-----------|-------------------------------------|----------------------------|
| ADR       | Architecture Decision Record，架构决策记录 | ADR                     |
| 宪法层 ADR   | ADR-0001~0005，系统根基不可推翻约束            | Constitutional ADR      |
| 治理层 ADR   | ADR-0000, 900~999，架构过程、CI、审批相关      | Governance ADR          |
| 结构/运行/技术层 | ADR-100~399，架构细化和技术落地               | Structure/Runtime/Technical ADR |
| RFC       | Request for Comments，架构决策草案         | RFC                     |
| 架构委员会     | 宪法层唯一审批主体                           | Architecture Board      |

---

---

## Decision（裁决）


> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 待补充规则

待补充...


---

---

## Enforcement（执法模型）


### 执行方式

待补充...


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **代码实现的技术细节**：不涉及代码如何编写、测试如何实现的具体技术规范
- **项目管理工具的选择**：不规定使用 Jira、Trello、GitHub Projects 还是其他项目管理工具
- **ADR 写作的文学风格**：不涉及写作技巧、修辞手法等文学层面的要求
- **团队协作的软技能**：不涉及沟通技巧、会议组织、冲突解决等软技能培训
- **多语言翻译流程**：不建立 ADR 的多语言翻译和维护流程
- **版本控制系统的内部机制**：不涉及 Git 的分支策略、合并策略等具体操作细节
- **ADR 工具链的选择**：不强制使用特定的 ADR 管理工具或插件
- **文档托管平台的选择**：不规定使用 GitHub Pages、GitBook 还是其他文档托管服务

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 流程违规
- ❌ **禁止绕过审批流程直接合并 ADR**：所有 ADR 必须经过规定的审批流程和同行评审
- ❌ **禁止未经架构委员会批准修改宪法层 ADR**：宪法层 ADR 变更需架构委员会全体一致同意
- ❌ **禁止一个 PR 包含多个不相关的 ADR**：每个 PR 只能包含一个 ADR 及其相关测试和文档

### 文档质量违规
- ❌ **禁止提交不完整的 ADR**：ADR 必须包含所有 ADR-902 规定的必需章节
- ❌ **禁止 ADR 缺少对应的架构测试**：标记为需要测试覆盖的 ADR 必须提供架构测试
- ❌ **禁止 ADR 使用模糊或歧义语言**：必须使用明确的裁决性语言（参考 ADR-901）

### 版本管理违规
- ❌ **禁止删除已发布的 ADR**：使用状态标记为 Deprecated/Superseded 而不是删除文件
- ❌ **禁止修改已分配的 ADR 编号**：ADR 编号一旦分配不得修改
- ❌ **禁止跳号或重复使用 ADR 编号**：保持编号的连续性和唯一性

### 治理违规
- ❌ **禁止口头约定替代正式 ADR**：所有架构决策必须文档化为正式 ADR
- ❌ **禁止以"临时方案"为由跳过 ADR 流程**：临时方案也需要通过 ADR 记录和审批
- ❌ **禁止在未废弃旧 ADR 的情况下创建冲突的新 ADR**：必须明确标记替代关系（Supersedes/Superseded By）


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - ADR 流程基于测试和 CI 机制
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - ADR 编号规则依赖编号宪法
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - ADR 文档标准依赖文档治理宪法

**被依赖（Depended By）**：
- [ADR-940：ADR 关系与溯源管理](./ADR-940-adr-relationship-traceability-management.md) - 关系管理依赖 ADR 流程
- [ADR-990：文档演进路线图](./ADR-990-documentation-evolution-roadmap.md) - 演进规划依赖 ADR 流程
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md)
- [ADR-930：代码审查与 ADR 合规自检流程](../governance/ADR-930-code-review-compliance.md)
- [ADR-952：工程标准与 ADR 分离边界](../governance/ADR-952-engineering-standard-adr-boundary.md)
- [ADR-980：ADR 生命周期一体化同步机制宪法](../governance/ADR-980-adr-lifecycle-synchronization.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md) - Copilot 在 ADR 流程中的角色
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档治理与 ADR 流程相关
- [ADR-980：ADR 生命周期一体化同步](./ADR-980-adr-lifecycle-synchronization.md) - 版本同步机制与 ADR 流程相关

---

---

## References（非裁决性参考）

**相关外部资源**：
- [ADR GitHub Organization](https://adr.github.io/) - ADR 社区标准和最佳实践
- [Architectural Decision Records by Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) - ADR 概念的原始论文
- [Markdown Any Decision Records (MADR)](https://adr.github.io/madr/) - MADR 模板规范
- [RFC 2119: Key words for use in RFCs](https://www.ietf.org/rfc/rfc2119.txt) - 规范性语言标准

**相关内部文档**：
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - ADR 测试和 CI 执行机制
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - ADR 编号规范和术语定义
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - ADR 文档编写标准
- [ADR-901：Warning 约束语义](./ADR-901-warning-constraint-semantics.md) - ADR 语义规范
- [ADR-902：ADR 标准模板与结构契约](./ADR-902-adr-template-structure-contract.md) - ADR 结构要求
- [ADR-905：执行级别分类](./ADR-905-enforcement-level-classification.md) - 执行级别定义


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
