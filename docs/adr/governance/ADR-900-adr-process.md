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

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


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


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
