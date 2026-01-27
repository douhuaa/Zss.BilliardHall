---
adr: ADR-901
title: "ADR 编写与结构统一宪法"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2025-01-20
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-901：ADR 编写与结构统一宪法

> **这是 ADR 体系的“写作宪法”。**
> 本 ADR 定义：**ADR 应该长什么样、必须包含什么、禁止出现什么，以及如何被执行与审计。**
> 任何不符合本 ADR 的文档，**不具备治理裁决力**。

---

## 聚焦内容（Focus）

本 ADR 聚焦于解决以下长期治理问题：

* ADR 文档**结构不一致**，导致理解成本和执行歧义
* 警告、约束、裁决语义**表达随意**，削弱权威性
* ADR 逐渐膨胀为“说明书 / 教程 / 杂文”
* 无法对 ADR 质量进行**自动化检查与治理**

**适用范围**：

* 所有 ADR（治理层 / 架构层 / 技术层 / 运行层）
* 所有状态（Draft / Accepted / Final / Superseded）

---

## 术语表（Glossary）

| 术语    | 定义                 | 英文对照                         |
| ----- | ------------------ | ---------------------------- |
| ADR   | 架构决策记录，具有裁决力的规范文档  | Architecture Decision Record |
| 裁决    | 对系统结构、行为具有强制约束力的规则 | Decision                     |
| 非裁决内容 | 不具备强制力，仅用于解释或背景说明  | Non-normative                |
| 执法    | 通过测试、流程或工具强制规则生效   | Enforcement                  |

---

## 裁决（Decision）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 规则列表

**ADR-901.1：ADR 必须使用统一模板结构**
所有 ADR **必须**严格遵循本 ADR 定义的章节顺序与命名，不允许随意增删裁决型章节。

**ADR-901.2：裁决只能写在 Decision 章节**
任何具有“必须 / 禁止 / 不允许 / 应当”语义的内容，**只能**出现在 `## 裁决（Decision）` 章节中。

**ADR-901.3：非裁决章节不得引入新约束**
Focus、References、History 等章节 **仅用于说明**，不得引入隐性规则。

**ADR-901.4：必须显式声明 Non-Goals**
每个 ADR **必须**包含“明确不管什么（Non-Goals）”章节，用于防止职责膨胀。

**ADR-901.5：无法执法的规则视为无效规则**
任何规则如果未在 Enforcement 中声明执行方式，**等同未定义**。

**ADR-901.6：关系必须显式声明**
每个 ADR **必须**声明其依赖、替代、被依赖关系，禁止隐式血缘。

**ADR-901.7：ADR 的裁决资格必须可被判定**
任何 ADR 在被引用、执行或作为治理依据前，
必须能够基于本 ADR 定义的判定标准，
被明确判定为 ✅Allowed / ⚠️Blocked / ❓Uncertain 之一。


### 判定输出语义

基于上述判定维度，ADR 在任一治理节点上 **必须**被判定为以下三态之一：

- ✅ Allowed：满足全部判定标准，可被依赖与执行
- ⚠️ Blocked：违反结构或裁决完整性要求，不具备裁决力
- ❓ Uncertain：结构合规但存在治理或语义不确定性，需人工裁决

判定状态不构成新的系统规则，仅用于治理与执行决策。

---

## 判定标准（Evaluation Criteria）

判定标准用于评估 ADR 是否具备以下能力：
- 结构合规性
- 裁决完整性
- 治理就绪度

## 执法模型（Enforcement）

> **没有执法模型的 ADR，只是意见，不是治理。**

### 测试映射

| 规则编号      | 执行级 | 执法方式                   |
| --------- | --- | ---------------------- |
| ADR-901.1 | L1  | ADR 模板结构校验（CI）         |
| ADR-901.2 | L1  | Decision 章节语义扫描        |
| ADR-901.3 | L2  | Review + 静态分析          |
| ADR-901.4 | L1  | Non-Goals 强制存在校验       |
| ADR-901.5 | L1  | Enforcement 映射完整性检查    |
| ADR-901.6 | L1  | ADR Relationship Check |
| ADR-901.7 | L2  | 人工审核与治理评估          |

---

## 明确不管什么（Non-Goals）

> **这是防止 ADR 腐化的关键防线。**

本 ADR **不负责**：

* 代码风格与格式化规则
* 命名审美与个人偏好
* 教学示例与新手引导
* 实现细节与具体技术选型
* 临时约定或过渡方案

---

## 结构标准（Structure Standard）

ADR **必须**按以下顺序组织，不允许重排：

1. Front Matter（adr / title / status / level ...）
2. Focus
3. Glossary（可为空但必须存在）
4. Decision
5. Enforcement
6. Non-Goals
7. Prohibited
8. Relationships
9. References
10. History

---

## 禁止行为（Prohibited）

ADR 中 **严禁**：

* 在 Decision 之外定义规则
* 使用模糊措辞（如“建议考虑”“尽量”）
* 混入教程、示例代码、操作指南
* 引用未声明关系的 ADR 作为前提
* 用“共识”“约定俗成”替代裁决

---

## 关系声明（Relationships）

**Depends On**：

* ADR-0000（架构治理宪法）

**Depended By**：

* 所有文档类 ADR（09xx）

**Supersedes**：

* 任何历史非结构化 ADR 约定

**Related**：

* ADR-0900（文档治理总则）

---

## 非裁决性参考（References）

* ISO/IEC/IEEE 42010
* Michael Nygard – Architecture Decision Records

---

## 版本历史（History）

| 版本  | 日期         | 变更说明   | 作者                 |
| --- | ---------- | ------ | ------------------ |
| 1.0 | 2025-01-20 | 初始正式版本 | Architecture Board |
