---
adr: ADR-946
title: "ADR 标题级别即语义级别约束"
status: Accepted
level: Governance
version: "2.0"
deciders: "Architecture Board"
date: 2026-02-03
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---


# ADR-946：ADR 标题级别即语义级别约束

> ⚖️ **本 ADR 定义 ADR 文档中标题层级与机器可解析语义之间的强制对应关系，用于防止解析歧义和治理规则污染。**

**状态**：✅ Accepted  
## Focus（聚焦内容）

- ADR 标题层级的语义含义
- 机器解析边界的明确化
- 模板 / 示例 / 说明性内容的结构约束
- 防止关系、自引用、版本等解析歧义

---

## Glossary（术语表）


| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 待补充 | 待补充 | TBD |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-946 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-946_<Rule>_<Clause>
> ```

---

### ADR-946_1：标题级别语义约束

- **ADR-946_1_1：标题级别即语义级别**

  - 每个 ADR 文件必须且仅有一个 `#` 标题
  - 正文中语义块 `##` 级别强制用于决策、关系声明、执法模型等
  - 子说明、示例、模板必须使用 `###` 或更低级别
  - **判定**：违反 → L1 阻断 CI / PR 合并

- **ADR-946_1_2：关键语义块标题约束**

  - `## Relationships（关系声明）`、`## Decision（裁决）`、`## Enforcement（执法模型）`、`## Glossary（术语表）`
  - 禁止在模板、示例、代码块中使用相同 `##` 标题
  - **判定**：违反 → L1 阻断 CI / PR 合并

---

### ADR-946_2：模板与示例结构约束

- **ADR-946_2_1：模板与示例必须避免解析歧义**

  - 示例/模板可使用：
    1. 英文或占位符标题（如 `## Relationships Example`）
    2. 降级标题 `###` 或更低
    3. 代码块内示例不使用语义 `##` 标题
  - **判定**：违反 → L1 阻断 CI / PR 合并


---

### ADR-946_3：解析工具约束

- **ADR-946_3_1：解析工具仅解析第一个 `##` 语义块并忽略代码块**
  - 使用正则严格匹配行首
  - 避免匹配模板/示例/非语义块
  - **判定**：违反 → L1 阻断 CI / PR 合并

---

## Enforcement（执法模型）

|规则编号|执行级|执法方式|Decision 映射|
|---|---|---|---|
|ADR-946_1_1|L1|自动验证标题语义|§ADR-946_1_1|
|ADR-946_1_2|L1|验证关键语义块|§ADR-946_1_2|
|ADR-946_2_1|L1|验证模板/示例结构|§ADR-946_2_1|
|ADR-946_3_1|L1|验证解析工具语义边界|§ADR-946_3_1|

**执行级别说明**：

- **L1（阻断级）**：违规直接导致 CI 失败，阻止合并
- **L2（警告级）**：记录告警，不阻断
- **L3（人工级）**：需要人工裁决

---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充

---

## Relationships（关系声明）

**Depends On**：
- [ADR-940：ADR 关系与溯源管理治理规范](./ADR-940-adr-relationship-traceability-management.md) - 关系解析依赖标题语义
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - ADR 文档结构规范
- [ADR-007：Agent 行为与权限宪法](../constitutional/ADR-007-agent-behavior-permissions-constitution.md) - ADR Reviewer 审查依赖的权限宪法

**Depended By**：
- [ADR-947：关系声明区的结构与解析安全规则](./ADR-947-relationship-section-structure-parsing-safety.md)

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - 同为 ADR 结构约束

---

## References（非裁决性参考）

- 待补充

---

## History（版本历史）


| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | 架构委员会 |
