---
adr: ADR-940
title: "ADR 关系与溯源管理治理规范"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "2.0"
author: "GitHub Copilot"
maintainer: "架构委员会"
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-940：ADR 关系与溯源管理治理规范

> ⚖️ **本 ADR 是所有 ADR 关系声明的治理规范，定义 ADR 之间关系的标准化管理机制。**

## Focus（聚焦内容）

- ADR 关系类型定义与标准化
- 关系声明的强制格式
- 关系双向一致性验证
- 循环依赖检测
- 全局关系图生成机制

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 关系声明 | ADR 文档中声明与其他 ADR 关系的章节 | Relationship Declaration |
| 依赖关系 | 本 ADR 基于另一 ADR 的规则或概念 | Dependency |
| 被依赖关系 | 其他 ADR 基于本 ADR | Depended By |
| 替代关系 | 本 ADR 取代另一个已废弃的 ADR | Supersedes |
| 被替代关系 | 本 ADR 已被新 ADR 取代 | Superseded By |
| 相关关系 | 与本 ADR 相关但不存在依赖 | Related |
| 双向一致性 | A 依赖 B 则 B 必须声明被 A 依赖 | Bidirectional Consistency |
| 关系图 | ADR 之间关系的可视化表示 | Relationship Map |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别**  
> 本 ADR 仅在 ADR-902 结构合规的前提下生效。不合规的 ADR 视为不存在，其关系不进入本 ADR 的裁决范围。  
> 所有可执法条款稳定 RuleId 格式：
>
> ```
> ADR-940_<Rule>_<Clause>
> ```

---

### ADR-940_1：关系声明章节要求

#### ADR-940_1_1 每个 ADR 必须包含关系声明章节

**规则**：

- 所有 ADR **必须**包含“关系声明（Relationships）”章节
- 位置：位于“决策（Decision）”章节之后
- 缺失或位置错误 → ❌违规

**判定**：

| 条件 | 判定 |
|------|------|
| 缺少关系声明章节 | ❌违规 |
| 章节位置不正确 | ❌违规 |
| 完整且位置正确 | ✅合规 |

---

### ADR-940_2：关系类型定义与约束

#### ADR-940_2_1 关系类型定义与使用约束

**规则**：

| 关系类型 | 定义 | 使用场景 | 强制双向 |
|---------|------|----------|----------|
| Depends On | 本 ADR 基于另一 ADR | ADR-005 依赖 ADR-001 | ✅ |
| Depended By | 其他 ADR 基于本 ADR | ADR-001 被 ADR-005 依赖 | ✅ |
| Supersedes | 本 ADR 取代已废弃 ADR | ADR-005-v2 替代 ADR-005-v1 | ✅ |
| Superseded By | 本 ADR 已被新 ADR 替代 | ADR-005-v1 被 ADR-005-v2 替代 | ✅ |
| Related | 与本 ADR 相关但不存在依赖 | ADR-001 与 ADR-002 | ❌ |

**裁决性约束**：

- 已废弃 ADR **禁止**被新 ADR 依赖
- 违反此规则的 PR **必须**拒绝合并

**判定**：

| 条件 | 判定 |
|------|------|
| 使用未定义的关系类型 | ❌违规 |
| 依赖/替代关系未双向声明 | ❌违规 |
| 新 ADR 依赖已废弃 ADR | ❌违规 |
| 关系类型正确且双向一致 | ✅合规 |

---

### ADR-940_3：关系一致性验证

#### ADR-940_3_1 双向一致性

**规则**：

- 依赖关系：A 依赖 B → B 必须声明被 A 依赖
- 替代关系：A 替代 B → B 必须声明被 A 替代
- 相关关系不要求双向，但建议双向声明
- CI 自动验证，违规阻断构建

**判定**：

| 条件 | 判定 |
|------|------|
| 双向一致性违反 | ❌阻断 |
| 双向一致 | ✅合规 |

---

### ADR-940_4：循环依赖治理

#### ADR-940_4_1 循环依赖禁止

**规则**：

- ADR 禁止形成循环依赖：
  - ADR-A → ADR-B → ADR-A
  - ADR-A → ADR-B → ADR-C → ADR-A
- CI 自动检测，违规阻断构建
- 解决方案：
  - 提取公共规则
  - 重新设计依赖
  - 改为相关关系

**判定**：

| 条件 | 判定 |
|------|------|
| 存在循环依赖 | ❌阻断 |
| 无循环依赖 | ✅合规 |

---

### ADR-940_5：关系可视化

#### ADR-940_5_1 全局关系图生成

**规则**：

- 位置：`docs/adr/ADR-RELATIONSHIP-MAP.md`
- 内容：Mermaid 图 + 表格
- 每次 ADR 变更后必须更新
- 分层关系图：宪法/治理/结构/运行/技术
- 状态关系图：Active / Superseded / Deprecated

**Mermaid 示例**：

```mermaid
graph TB
    ADR0001[ADR-001: 模块化架构] --> ADR0005[ADR-005: 交互模型]
    ADR0005 --> ADR0201[ADR-201: Handler 生命周期]

    style ADR0001 fill:#90EE90
    style ADR0005 fill:#87CEEB
    style ADR0201 fill:#FFD700
````

**判定**：

|条件|判定|
|---|---|
|关系图缺失或过时|⚠️警告|
|关系图准确|✅合规|

---

## Enforcement（执法模型）

|规则编号|执行级|测试/手段|Decision 映射|
|---|---|---|---|
|ADR-940_1_1|L1|`ADR_940_1_1_Relationship_Section_Required`|§ADR-940_1_1|
|ADR-940_2_1|L1|`ADR_940_2_1_Relationship_Types_Valid`|§ADR-940_2_1|
|ADR-940_2_1 已废弃约束|L1|`ADR_940_2_1_No_Superseded_ADR_Dependency`|§ADR-940_2_1|
|ADR-940_3_1|L1|`ADR_940_3_1_Bidirectional_Consistency`|§ADR-940_3_1|
|ADR-940_4_1|L1|`ADR_940_4_1_No_Circular_Dependencies`|§ADR-940_4_1|
|ADR-940_5_1|L2|`scripts/generate-adr-relationship-map.sh`|§ADR-940_5_1|

**执行级别说明**：

- L1（结构违规）：违反 → CI 阻断，PR 无法合并
- L2（治理警告）：仅记录，不阻断合并

**裁决边界**：

- 关系声明缺失 → 阻断
- 关系类型错误 → 阻断
- 新 ADR 依赖废弃 ADR → 阻断
- 双向一致性违规 → 阻断
- 循环依赖 → 阻断
- 关系图未生成 → 警告

---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- ADR 文档内容的语义校验（由 ADR-902 管理）
- ADR 文档的审批流程（由 ADR-900 管理）
- 文档编写的具体规范（由 ADR-008 管理）
- 关系图的具体渲染样式（由工具自行决定）

---

## Prohibited（禁止行为）

以下行为明确禁止：

- **禁止**创建循环依赖关系
- **禁止**在 Final/Active ADR 中缺失 Relationships 章节
- **禁止**使用未定义的关系类型
- **禁止**在关系声明中使用相对路径
- **禁止**新 ADR 依赖已废弃的 ADR

---

## Relationships（关系声明）

### Depends On

- [ADR-900：架构测试与 CI 治理元规则](https://chatgpt.com/c/ADR-900-architecture-tests.md)

- [ADR-008：文档编写与维护宪法](https://chatgpt.com/constitutional/ADR-008-documentation-governance-constitution.md)

- [ADR-900：ADR 新增与修订流程](https://chatgpt.com/c/ADR-900-architecture-tests.md)


### Depended By

- [ADR-945：ADR 全局时间线与演进视图](https://chatgpt.com/c/ADR-945-adr-timeline-evolution-view.md)

- [ADR-946：ADR 标题级别即语义级别约束](https://chatgpt.com/c/ADR-946-adr-heading-level-semantic-constraint.md)

- [ADR-947：关系声明区的结构与解析安全规则](https://chatgpt.com/c/ADR-947-relationship-section-structure-parsing-safety.md)

- [ADR-955：文档搜索与可发现性优化](https://chatgpt.com/c/ADR-955-documentation-search-discoverability.md)

- [ADR-980：ADR 生命周期一体化同步机制治理规范](https://chatgpt.com/c/ADR-980-adr-lifecycle-synchronization.md)


### Supersedes

无

### Superseded By

无

### Related

- [ADR-006：术语与编号宪法](https://chatgpt.com/constitutional/ADR-006-terminology-numbering-constitution.md)

- [ADR-008：文档编写与维护宪法](https://chatgpt.com/constitutional/ADR-008-documentation-governance-constitution.md)

- [ADR-946：标题语义约束](https://chatgpt.com/c/ADR-946-adr-heading-level-semantic-constraint.md)


---

## References（非裁决性参考）

**工具和脚本**：

- `scripts/verify-adr-relationships.sh`

- `scripts/check-relationship-consistency.sh`

- `scripts/detect-circular-dependencies.sh`

- `scripts/generate-adr-relationship-map.sh`


**相关文档**：

- [ADR 关系图](https://chatgpt.com/ADR-RELATIONSHIP-MAP.md)

- [ADR 模板](https://chatgpt.com/templates/adr-template.md)

---

## History（版本历史）

|版本|日期|变更说明|作者|
|---|---|---|---|
|2.0|2026-02-03|对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系|Architecture Board|
|1.0|2026-01-26|初始版本，定义 ADR 关系管理机制|GitHub Copilot|
