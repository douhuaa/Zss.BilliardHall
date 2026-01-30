---
adr: ADR-947
title: "关系声明区的结构与解析安全规则"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---


# ADR-947：关系声明区的结构与解析安全规则

> ⚖️ **本 ADR 是防止解析歧义、自指、模板污染、循环链放大的最小可执行规则集。**

**状态**：✅ Accepted  
## Focus（聚焦内容）

- 关系声明区的唯一性与边界
- ADR 编号的使用约束
- 同编号多文档禁止
- 循环依赖的声明约束
- 机器解析的结构保证

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 关系声明区 | ADR 中声明与其他 ADR 关系的章节 | Relationship Declaration Section |
| 解析歧义 | 机器解析工具无法准确识别语义边界 | Parsing Ambiguity |
| 模板污染 | 模板示例被误判为实际依赖关系 | Template Pollution |
| 自指 | ADR 声明依赖自身 | Self-Reference |
| 循环声明 | 双向依赖同时出现在关系声明中 | Bidirectional Assertion |

---

---

## Decision（裁决）

### 唯一顶级关系区原则（条款 1）

**规则**：

每个 ADR **必须**且**只能**包含一个顶级二级标题（`## Relationships（关系声明）`）。

**禁止**：
- ❌ 出现第二个同名 `## 关系声明`
- ❌ 模板、示例、说明中使用同级标题
- ❌ 任何形式的关系声明章节重复

**允许**：
- ✅ 模板示例**必须**降级为 `###` 或更低

**裁决理由**：关系区是机器裁决入口，只允许一个 authoritative source。

**判定**：
- ❌ 文档中存在多个 `## 关系声明`
- ❌ 模板使用 `## 关系声明` 标题
- ✅ 仅有一个顶级关系声明区

---

### 关系区边界即标题边界（条款 2）

**规则**：

"关系声明"区的有效内容**仅**存在于从 `## Relationships（关系声明）` 到下一个 `##` 或 `#` 标题之间。

**约束**：
- ❌ 任何 `###` / `####` 不允许作为关系声明的延伸内容
- ❌ 关系区内**禁止**出现任何 ADR 编号说明性文字
- ✅ 仅包含依赖、被依赖、替代、被替代、相关五类列表

**裁决理由**：解决 sed/grep 永不停止问题，确保边界清晰。

**判定**：
- ❌ 关系区内包含说明性段落
- ❌ 关系区内包含子章节
- ✅ 关系区仅包含列表项

---

### 禁止 ADR 编号出现在非声明语义中（条款 3）

**规则**：

在**非"关系声明"区**：

**禁止**：
- ❌ 出现形如 `ADR-XXXX` 的字符串（XXXX 为具体数字）

**允许**：
- ✅ 若必须说明，**必须**使用占位符（如 `ADR-####`、`ADR-NNNN`）

**强制防止**：
- 章节标题污染
- 模板被误判为真实依赖
- sed / grep / awk 误抽取

**判定**：
- ❌ 决策章节中出现 `ADR-0001` 等具体编号
- ❌ 示例代码中出现 `ADR-940` 等具体编号
- ✅ 使用 `ADR-####` 作为占位符

---

### 禁止同编号多文档（条款 4）

**规则**：

同一个 ADR 编号：

**禁止**：
- ❌ 拆分为多个文件（如 `ADR-0005-xxx.md` + `ADR-0005-yyy.md`）

**允许**：
- ✅ 补充内容**必须**以内嵌章节存在于主文件中
- ✅ 或使用不同编号（如补充文档使用新编号）

**裁决理由**：直接消除 `ADR-0005 → ADR-0005` 这种自指。

**判定**：
- ❌ 存在 `ADR-0005-A.md` 和 `ADR-0005-B.md`
- ✅ 仅存在一个 `ADR-0005-xxx.md`

---

### 禁止显式循环声明（条款 5）

**规则**：

在"关系声明"中：

**禁止**：
- ❌ 同时出现：
  - A → B（依赖）
  - B → A（被依赖）

**处理方案**：
若存在历史逻辑闭环：
- ✅ **只能单向声明**依赖关系
- ✅ 另一侧**必须**使用"相关关系"或解释性说明
- ✅ 不得进入关系声明的依赖/被依赖区

**裁决理由**：循环不是"事实"，是治理失败。

**判定**：
- ❌ ADR-A 依赖 ADR-B，且 ADR-B 依赖 ADR-A
- ✅ ADR-A 依赖 ADR-B，ADR-B 将 ADR-A 列为相关

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
- [ADR-940：ADR 关系与溯源管理治理规范](./ADR-940-adr-relationship-traceability-management.md) - 关系管理的基础规则
- [ADR-946：ADR 标题级别即语义级别约束](./ADR-946-adr-heading-level-semantic-constraint.md) - 标题语义约束
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档结构规范

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0006：术语与编号宪法](../constitutional/ADR-0006-terminology-numbering-constitution.md) - ADR 编号规范

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
