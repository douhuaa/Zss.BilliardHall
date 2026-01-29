---
adr: ADR-902
title: "ADR 标准模板与结构契约"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-01-28
version: "1.0"
maintainer: "Architecture Board"
primary_enforcement: L2
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-902：ADR 标准模板与结构契约

> **这是 ADR 体系的“结构接口定义”。**
> 本 ADR 不讨论规则对不对、严不严，
> 只裁定一件事：**一个 ADR 文档是否“长得合法”，从而具备被治理系统消费的资格。**
> 任何不符合 ADR-902 的 ADR，视为不存在。

---

## Focus（聚焦内容）

本 ADR 聚焦解决以下问题：

- ADR 文档**结构随意**，无法自动化校验
- Front Matter 不统一，**无法索引、审计、替代**
- 章节缺失或顺序混乱，**削弱裁决可读性**
- 模板靠“约定俗成”，**CI 无法阻断劣化**

**本 ADR 的目标只有一个**：

> **让 ADR 成为一种“可被机器和人同时理解的治理工件”。**

---

## Glossary（术语表）

| 术语           | 定义                         | 英文对照                  |
| ------------ | -------------------------- | --------------------- |
| ADR 模板       | ADR 文档必须遵循的最小结构契约          | ADR Template          |
| Front Matter | ADR 文件头部的元信息区块             | Front Matter          |
| 结构合规         | ADR 是否满足模板与顺序要求            | Structural Compliance |
| 治理接口         | 可被 CI / Review / 工具消费的文档结构 | Governance Interface  |

---

## Decision（裁决）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**
>>
> 每条规则必须独立为三级标题，格式统一：
>
> `### ADR-902.X:L? <规则标题>（中文别名可选）`
>
> 例如：
>
> `### ADR-902.1:L1 规则条目必须独立编号（独立三级标题）`
---

### ADR-902.1:L1 规则条目必须独立编号

- 每条规则必须作为独立三级标题存在。
- 避免合并多条规则，确保 CI 能精确扫描。
- 标题格式严格遵守：`### ADR-902.X:L? <规则标题>（中文别名可选）`

### ADR-902.2:L1 统一模板结构

- 所有 ADR 文档 **必须**符合本 ADR 定义的：
  - Front Matter 字段规范
  - 章节集合
  - 章节顺序

- 任何不符合模板的 ADR：
  - **不具备裁决力**
  - **不得被其他 ADR 依赖**
  - **不得作为治理依据**

---

### ADR-902.3:L1 标准 Front Matter

每个 ADR 文件 **必须**以 YAML Front Matter 开头，并至少包含以下字段：

```yaml
adr: ADR-xxx
title: "<标题>"
status: Draft | Accepted | Final | Superseded
level: Constitutional | Governance | Structure | Runtime | Technical
deciders: "<裁决主体>"
date: YYYY-MM-DD
version: "<语义版本>"
maintainer: "<维护者>"
reviewer: "<审核者>"
supersedes: ADR-xxx | null
superseded_by: ADR-xxx | null
```

**约束**：

- ❌ 不允许缺失 `adr / title / status / level`
- ❌ 不允许使用自由文本替代枚举值
- ❌ 不允许在正文中“补充说明”元信息

---

### ADR-902.4:L1 包含完整章节集合

- 每个 ADR **必须**包含以下章节，**英文名称为裁决关键字（Canonical Name）**，中文名称仅作显示别名（Alias）：

|顺序|Canonical Name|中文别名（推荐）|
|---|---|---|
|1|Focus|聚焦内容|
|2|Glossary|术语表|
|3|Decision|裁决|
|4|Enforcement|执法模型|
|5|Non-Goals|明确不管什么|
|6|Prohibited|禁止行为|
|7|Relationships|关系声明|
|8|References|非裁决性参考|
|9|History|版本历史|

**说明**：
  - 所有 Canonical Name **必须存在且顺序固定**
  - 中文别名 **可选，但一旦使用必须与 Canonical Name 成对出现**
  - 不允许仅使用中文标题而缺失 Canonical Name
  - 不允许合并、重命名、重排上述章节

---

### ADR-902.5:L1 Decision 严格隔离

- 所有 约束性规则只能出现在 ## Decision。
- 非 Decision 章节不得出现 MUST / 禁止 / 不允许 等裁决语义。
- 隐性约束禁止出现在非 Decision 章节。

---

### ADR-902.6:L1 ADR 模板不承担语义裁决职责

- 不定义 Warning / Constraint / Notice。
- 不定义规则执行级别（L1/L2/L3）。
- 语义裁决由 ADR-901 与 ADR-905 执行。

---

## Enforcement（执法模型）

> **模板本身不做语义裁决，只验证结构与格式。**

### 测试映射
| 规则编号      | 执行级 | 执法方式                      |
| --------- | --- | ------------------------- |
| ADR-902.1 | L1  | CI 扫描三级标题独立性              |
| ADR-902.2 | L1  | CI 模板完整性校验                |
| ADR-902.3 | L1  | Front Matter 字段 + 格式校验    |
| ADR-902.4 | L1  | 章节存在性与顺序检查                |
| ADR-902.5 | L2  | Decision 语义扫描 + 人工 Review |
| ADR-902.6 | L1  | 模板不承担语义裁决验证               |

### 执行时机

- **CI 阶段**：结构违规直接阻断
- **PR Review**：L2 违规需人工裁定
- **审计阶段**：历史 ADR 结构一致性检查

---

## Non-Goals（明确不管什么）

- ADR 内容质量高低
- 架构决策是否正确
- 示例代码与教学说明
- 团队沟通与培训方式
- 文档美观与排版风格

---

## Prohibited（禁止行为）

- 缺失必需章节
- 在 Decision 之外写裁决
- 使用非标准 Front Matter
- 通过注释或正文绕过模板约束
- 创建“特殊 ADR 例外模板”

---

## Relationships（关系声明）

**Depends On**：

- [ADR-901：ADR 语义宪法](ADR-901-warning-constraint-semantics.md) - 定义 ADR 语义与裁决资格
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档治理宪法

**Depended By**：

- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md) - 测试组织遵循 ADR 结构
- [ADR-905：架构约束分类与裁决实施映射](ADR-905-enforcement-level-classification.md) - 约束分类基于本 ADR 的结构
- 所有 ADR（强制）

**Related**：

- [ADR-905](ADR-905-enforcement-level-classification.md) - 执行级别分类

---

## References（非裁决性参考）

- ISO/IEC/IEEE 42010
- Michael Nygard – Architecture Decision Records

---


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


## History（版本历史）

| 版本  | 日期         | 变更说明                  | 作者                 |
| --- | ---------- | --------------------- | ------------------ |
| 1.0 | 2026-01-27 | 初始正式版本，定义 ADR 模板与结构契约 | Architecture Board |
