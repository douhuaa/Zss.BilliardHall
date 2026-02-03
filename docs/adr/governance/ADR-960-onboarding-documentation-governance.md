---
adr: ADR-960
title: "Onboarding 文档治理规范"
status: Accepted
level: Governance
deciders: "Architecture Board"
date: 2026-02-03
version: "2.0"
maintainer: "架构委员会"
primary_enforcement: L1
reviewer: "@douhuaa"
supersedes: null
superseded_by: null
---

# ADR-960：Onboarding 文档治理规范

> ⚖️ **本 ADR 定义新成员（开发者 / Reviewer / Maintainer）进入项目时所使用的 Onboarding 文档的权威边界、结构标准与维护规则，确保“快速上手但不产生新裁决”。**

**状态**：✅ Accepted  

## Focus（聚焦内容）

- Onboarding 文档的权威定位  
- Onboarding 与 ADR / Guide / FAQ 的分离边界  
- Onboarding 文档的强制结构  
- 维护与失效治理机制  

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| Onboarding | 帮助新成员理解项目和规则的引导文档 | Onboarding |
| 新成员 | 首次参与该仓库的开发者或维护者 | New Contributor |
| 权威边界 | 文档是否具备裁决力的明确界限 | Authority Boundary |
| 快速路径 | 最短可用的上手流程 | Fast Path |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-960 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-960_<Rule>_<Clause>
> ```

---

### ADR-960_1：Onboarding 文档的权威定位（Rule）

#### ADR-960_1_1 不是裁决性文档
- Onboarding 文档 **不是裁决性文档**

#### ADR-960_1_2 不得定义架构约束
- Onboarding 文档 **不得**：
  - 定义架构约束
  - 引入“必须 / 禁止”级别的新规则
  - 覆盖或修改任何 ADR 裁决

#### ADR-960_1_3 唯一合法职责
- Onboarding 的唯一合法职责：
  - **告诉你“先看什么、怎么走、不踩雷”**

#### ADR-960_1_4 权威层级
- **权威层级**：
```
ADR（裁决，最高权威）
↓ 被引用
Guide / FAQ / Standard（解释与细化）
↓ 被串联
Onboarding（导航与入口，不得裁决）
```

**判定**：
- ❌ Onboarding 中出现“模块必须隔离”等裁决性表述
- ❌ Onboarding 直接替代 ADR 解释规则
- ✅ Onboarding 仅引用并指路 ADR / Guide

---

### ADR-960_2：Onboarding 与其他文档的分离边界（Rule）

#### ADR-960_2_1 内容类型限制
- | 内容类型 | 是否允许出现在 Onboarding |
  |---------|---------------------------|
  | 架构约束定义 | ❌ 禁止 |
  | ADR 裁决内容 | ❌ 禁止 |
  | 操作步骤摘要 | ✅ 允许（不得完整展开） |
  | 文档导航与顺序 | ✅ 允许 |
  | 常见新手误区 | ✅ 允许 |
  | 示例代码 | ❌ 禁止（必须跳转 Case / Example） |

#### ADR-960_2_2 核心原则
- **核心原则**：
  > Onboarding 只解决三个问题：  
  > 1️⃣ 我是谁（这个项目是什么）  
  > 2️⃣ 我先看什么  
  > 3️⃣ 我下一步去哪  

**判定**：
- ❌ Onboarding 包含完整操作教程
- ❌ Onboarding 内嵌大段代码
- ✅ Onboarding 只做导航与摘要

---

### ADR-960_3：Onboarding 文档的强制结构（Rule）

#### ADR-960_3_1 强制结构模板
- 所有 Onboarding 文档 **必须**遵循以下结构：

```markdown
# Onboarding：项目名称

## 你正在进入什么系统
- 项目定位
- 架构风格一句话说明

## 快速上手路径（Fast Path）
1. 必读 ADR（链接）
2. 必读 Guide（链接）
3. 推荐 Case（链接）

## 新成员高频踩雷区
- ❌ 常见错误 1（指向 ADR）
- ❌ 常见错误 2（指向 Case）

## 常用入口
- ADR 索引
- Case 索引
- 工程标准索引

## 不在这里解决的问题
- 架构裁决 → ADR
- 操作细节 → Guide
- 示例代码 → Case
```

**判定**：

* ❌ 缺失 Fast Path
* ❌ 未明确“这里不管什么”
* ✅ 结构完整、导航清晰

---

### ADR-960_4：Onboarding 文档维护与失效治理（Rule）

#### ADR-960_4_1 绑定 ADR 演进
- Onboarding 文档 **必须绑定 ADR 演进**
- 当发生以下事件时，**必须评估是否更新 Onboarding**：
  - 新 ADR 被采纳
  - ADR 结构发生重大调整
  - 新的核心 Case 出现
- 至少 **每半年一次** 进行有效性审计

#### ADR-960_4_2 失效处理
- **失效处理**：
  - 发现内容误导 → 立即修复
  - 无法及时修复 → 标记 `[可能过时]`
  - 不允许长期错误但“懒得改”

**判定**：

* ❌ Onboarding 引用已废弃 ADR
* ❌ 超过 6 个月无人维护
* ✅ 与当前 ADR 体系一致

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-960 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-960_1_1** | L1 | 文档扫描裁决性用语 | §ADR-960_1_1 |
| **ADR-960_1_2** | L1 | 文档扫描裁决性用语 | §ADR-960_1_2 |
| **ADR-960_1_3** | L1 | 文档扫描裁决性用语 | §ADR-960_1_3 |
| **ADR-960_1_4** | L1 | 文档扫描裁决性用语 | §ADR-960_1_4 |
| **ADR-960_2_1** | L1 | 示例/代码存在性检查 | §ADR-960_2_1 |
| **ADR-960_2_2** | L1 | 示例/代码存在性检查 | §ADR-960_2_2 |
| **ADR-960_3_1** | L1 | 结构完整性检查 | §ADR-960_3_1 |
| **ADR-960_4_1** | L1 | ADR 变更触发校验 | §ADR-960_4_1 |
| **ADR-960_4_2** | L1 | ADR 变更触发校验 | §ADR-960_4_2 |

---

## Non-Goals（明确不管什么）

* 不负责教学深度
* 不负责代码正确性
* 不替代 README / Guide / Case
* 不解决资深成员的问题

---

## Prohibited（禁止行为）

* 在 Onboarding 中定义新规则
* 将 Onboarding 作为 ADR 的“简化版”
* 把 Onboarding 当 Wiki 堆内容

---

## Relationships（关系声明）

**依赖（Depends On）**：

* [ADR-950：指南与 FAQ 文档治理规范](ADR-950-guide-faq-documentation-governance.md)
* [ADR-951：案例库管理规范](ADR-951-case-repository-management.md)
* [ADR-955：文档搜索与可发现性优化](ADR-955-document-search-discoverability.md)

**被依赖（Depended By）**：

* 新成员流程
* 项目 README 入口

**替代 / 被替代**：无

**相关（Related）**：

* [ADR-910：README 编写与维护治理规范](ADR-910-readme-governance-constitution.md)

---

## References（非裁决性参考）

### 模板

* [Onboarding 模板](../../templates/onboarding-template.md)

### 相关文档

* [新成员快速路径示例](../../guides/onboarding-fast-path-example.md)

---

## History（版本历史）

| 版本  | 日期         | 变更说明 |
| --- | ---------- | ---- |
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | 架构委员会 |
| 1.0 | 2026-02-03 | 初始版本 |

---

### 直说一句（强观点）

- **没有 ADR-960，你的文档体系一定会“对新人不友好，对老人更痛苦”**
- Onboarding 一旦不被治理，必然退化成：  
  👉 规则副本  
  👉 过时说明  
  👉 架构污染源  

下一步我非常建议你做的是：

👉 **ADR-960 × ADR-955 → 自动生成“新成员入口页”**  
👉 并把它挂到 README 的唯一入口
