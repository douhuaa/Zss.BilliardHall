---
adr: ADR-950
title: "指南与 FAQ 文档治理规范"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-01-30
version: "1.1"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---


# ADR-950：指南与 FAQ 文档治理规范

> ⚖️ **本 ADR 是所有非裁决性文档（Guide、FAQ、Case、Standard）的治理规范，定义其与 ADR 的分离边界。**

**状态**：✅ Accepted（已采纳）  
## Focus（聚焦内容）

- 文档类型定义与权威关系
- ADR 与非裁决性文档的分离边界
- 各类文档的结构标准
- 定期审计机制
- 禁止行为清单

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 裁决性文档 | 定义架构约束和规则的文档（ADR） | Decision Document |
| 非裁决性文档 | 解释、指导、示例类文档 | Non-Decision Document |
| Guide | 操作指南，解释如何应用 ADR | Guide |
| FAQ | 常见问题解答 | Frequently Asked Questions |
| Case | 实践案例库 | Case Study |
| Standard | 工程标准，细化 ADR 实施细节 | Engineering Standard |
| 分离边界 | ADR 与其他文档的内容划分界限 | Separation Boundary |

---

---

## Decision（裁决）

### 文档类型定义与权威关系（ADR-950.1）

**规则**：

| 类型 | 目的 | 允许定义规则 | 更新频率 | 位置 | 裁决力 |
|------|------|------------|----------|------|--------|
| **ADR** | 定义约束 | ✅ 是 | 很少 | `docs/adr/` | ✅ 有 |
| **Guide** | 解释应用 | ❌ 否 | 随 ADR 变更 | `docs/guides/` | ❌ 无 |
| **FAQ** | 解答问题 | ❌ 否 | 按需 | `docs/faqs/` | ❌ 无 |
| **Case** | 记录案例 | ❌ 否 | 按需 | `docs/cases/` | ❌ 无 |
| **Standard** | 工程标准 | ⚠️ 有限 | 按需 | `docs/engineering-standards/` | ⚠️ 有限 |

**权威关系**：
```
ADR（宪法层，最高权威）
 ↓ 解释和指导
Guide（操作指南）
 ↓ 快速查询
FAQ（常见问题）
 ↓ 实践示例
Case（案例库）

ADR ← 基于
Standard（工程标准，不得引入新约束）
```

**核心原则**：
> ADR 定义"是什么"和"必须/禁止"  
> Guide 解释"怎么做"  
> FAQ 解答"为什么"和"如何理解"  
> Case 展示"实践示例"  
> Standard 细化"实施细节"但不得超越 ADR

**判定**：
- ❌ Guide 中定义新架构规则
- ❌ FAQ 中覆盖 ADR 决策
- ✅ Guide 引用 ADR 并提供操作步骤

---

### ADR 与非裁决性文档的分离边界（ADR-950.2）

**规则**：

**何时写 ADR**：
- ✅ 定义架构约束
- ✅ 引入新术语
- ✅ 需要测试执行的规则
- ✅ 不可协商的决策
- ✅ 影响系统全局的决定

**何时写 Guide**：
- ✅ 解释如何应用 ADR
- ✅ 操作步骤说明
- ✅ 工具使用教程
- ✅ 多步骤流程

**何时写 FAQ**：
- ✅ 解答常见疑问
- ✅ 澄清容易混淆的概念
- ✅ 快速参考
- ✅ 故障排查

**何时写 Case**：
- ✅ 记录实际问题和解决方案
- ✅ 展示最佳实践
- ✅ 反模式警告
- ✅ 复杂场景示例

**何时写 Standard**：
- ⚠️ 细化 ADR 的实施细节
- ⚠️ 工具配置标准
- ⚠️ 代码风格约定
- ⚠️ 必须基于 ADR，不能引入新约束

**决策树**：
```
问题：需要文档化某个内容
├─ 是否定义架构约束？
│  ├─ 是 → 写 ADR
│  └─ 否 ↓
├─ 是否需要测试执行？
│  ├─ 是 → 写 ADR
│  └─ 否 ↓
├─ 是否解释如何操作？
│  ├─ 是 → 写 Guide
│  └─ 否 ↓
├─ 是否解答常见问题？
│  ├─ 是 → 写 FAQ
│  └─ 否 ↓
├─ 是否记录实践案例？
│  ├─ 是 → 写 Case
│  └─ 否 ↓
└─ 是否细化工程实施？
   ├─ 是 → 写 Standard
   └─ 否 → 重新评估是否需要文档化
```

**判定**：
- ❌ 在 Guide 中定义"模块必须隔离"
- ❌ 在 ADR 中包含详细操作步骤
- ✅ ADR 定义规则，Guide 提供操作步骤

---

### 文档结构标准（ADR-950.3）

**规则**：

**Guide 标准结构**：
```markdown
# 指南：标题

**目的**：[一句话说明]  
**相关 ADR**：[ADR-XXXX 链接]  
**前置条件**：[需要了解的内容]  
**适用场景**：[何时使用本指南]

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
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 基础文档规范

**被依赖（Depended By）**：
- [ADR-951：案例库管理规范](ADR-951-case-repository-management.md) - Case 类型细化
- [ADR-952：工程标准与 ADR 分离边界](ADR-952-engineering-standard-adr-boundary.md) - Standard 类型细化
- [ADR-960：Onboarding 文档治理规范](ADR-960-onboarding-documentation-governance.md) - Onboarding 文档规范

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-910：README 编写与维护治理规范](ADR-910-readme-governance-constitution.md) - README 是另一类非裁决性文档

---

---

## References（非裁决性参考）

### 模板

- [Guide 模板](../../templates/guide-template.md)
- [FAQ 模板](../../templates/faq-template.md)
- [Case 模板](../../templates/case-template.md)
- [Standard 模板](../../templates/standard-template.md)

### 相关文档

- [文档类型决策树](../../guides/document-type-decision-tree.md)
- [审计清单](../../templates/documentation-audit-checklist.md)

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
