---
adr: ADR-XXXX
title: "〈裁决型标题〉"
status: Draft
level: Constitutional
deciders: "Architecture Board"
date: YYYY-MM-DD
version: "1.0"
supersedes: null
superseded_by: null
---

# ADR-XXXX：〈裁决型标题〉

> ⚖️ 权威声明：〈一句话说明本 ADR 的权威性和适用范围〉

---

## 聚焦内容（Focus）

本 ADR 聚焦于解决以下问题：

* 〈问题 1〉
* 〈问题 2〉
* 〈问题 3〉

**适用范围**：

* 〈明确列出适用的模块 / 层 / 仓库〉

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|-----|-----|---------|
| 〈术语1〉 | 〈定义〉 | 〈English〉 |
| 〈术语2〉 | 〈定义〉 | 〈English〉 |

---

## 裁决（Decision）

> ⚠️ **本节是唯一裁决来源，其他章节不得产生新规则。**

### 规则列表

**ADR-XXXX.1：〈规则标题〉**
〈使用"必须 / 禁止 / 不允许 / 应当"等明确语义描述规则〉

**ADR-XXXX.2：〈规则标题〉**
〈规则描述〉

**推荐格式：**

* ❗ 必须（MUST）/ 禁止（MUST NOT）/ 应当（SHALL）/ 不应（SHALL NOT）
* ❗ 违反即为架构违规

**示例：**

* ❌ Platform **禁止**依赖 Application 或 Host
* ❌ 模块**禁止**直接引用其他模块
* ✅ 所有标记【必须架构测试覆盖】的 ADR **必须**至少有一个架构测试

> 规则段落 **≤ 1 页**，超过就是设计说明，不是 ADR。

---

## 执法模型（Enforcement）

> **没有执法模型的 ADR，只是意见，不是治理。**

### 测试映射

> 仅列出**裁决级**测试，不列示例。

| 规则编号 | 执行级 | 测试 / 手段 |
|---------|-------|-----------|
| ADR-XXXX.1 | L1 | `XXX_Should_Not_Depend_On_YYY` |
| ADR-XXXX.2 | L2 | Roslyn Analyzer / Review |
| ADR-XXXX.3 | L3 | ARCH-GATE |

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 代码风格与格式化规则
* 命名审美与个人偏好
* 教学示例与新手引导
* 实现细节与具体技术选型
* 临时约定或过渡方案

---

## 禁止行为（Prohibited）

ADR 中 **严禁**：

* 在 Decision 之外定义规则
* 使用模糊措辞（如"建议考虑""尽量"）
* 混入教程、示例代码、操作指南
* 引用未声明关系的 ADR 作为前提
* 用"共识""约定俗成"替代裁决

---

## 关系声明（Relationships）

**依赖（Depends On）**：

* 〈ADR 编号及链接〉

**被依赖（Depended By）**：

* 〈ADR 编号及链接〉

**替代（Supersedes）**：

* 〈ADR 编号及链接〉

**被替代（Superseded By）**：

* 〈ADR 编号及链接〉

**相关（Related）**：

* 〈ADR 编号及链接〉

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

* 〈相关标准或文档〉
* 〈背景材料〉
* 〈历史讨论〉

---

## 版本历史（History）

| 版本 | 日期 | 变更说明 | 作者 |
|-----|------|---------|------|
| 1.0 | YYYY-MM-DD | 初始版本 | 〈作者〉 |
