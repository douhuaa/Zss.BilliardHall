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

**状态**：✅ Accepted  

## Focus（聚焦内容）

- 文档类型定义与权威关系  
- ADR 与非裁决性文档的分离边界  
- 各类文档的结构标准  
- 定期审计机制  
- 禁止行为清单  

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

## Decision（裁决）

> 🔒 **统一铁律**：  
> 所有非裁决性文档治理规则必须映射到 Clause，并可通过 ArchitectureTests 自动验证其结构和分离边界。

---

### ADR-950_1：文档类型定义与权威关系（Rule）

#### ADR-950_1_1 类型权威性
- ADR：唯一裁决性文档，定义架构约束  
- Guide / FAQ / Case：非裁决性文档，不得定义新规则  
- Standard：有限裁决力，仅细化实施细节  
- 判定：
  - ❌ Guide 中定义新架构规则  
  - ❌ FAQ 中覆盖 ADR 决策  
  - ✅ Guide 引用 ADR 并提供操作步骤  

#### ADR-950_1_2 权威层级映射
```text
ADR（最高权威）
↓ 解释和指导
Guide
↓ 快速查询
FAQ
↓ 实践示例
Case
ADR ← 基于
Standard（不得引入新约束）
```

---

### ADR-950_2：ADR 与非裁决性文档的分离边界（Rule）

#### ADR-950_2_1 写 ADR 条件
- ✅ 定义架构约束  
- ✅ 引入新术语  
- ✅ 需测试执行的规则  
- ✅ 不可协商决策  
- ✅ 影响系统全局  

#### ADR-950_2_2 写 Guide 条件
- ✅ 解释如何应用 ADR  
- ✅ 操作步骤说明  
- ✅ 工具使用教程  
- ✅ 多步骤流程  

#### ADR-950_2_3 写 FAQ 条件
- ✅ 解答常见疑问  
- ✅ 澄清概念  
- ✅ 快速参考  
- ✅ 故障排查  

#### ADR-950_2_4 写 Case 条件
- ✅ 记录实际问题与解决方案  
- ✅ 展示最佳实践  
- ✅ 反模式警告  
- ✅ 复杂场景示例  

#### ADR-950_2_5 写 Standard 条件
- ⚠️ 细化 ADR 的实施细节  
- ⚠️ 工具配置标准  
- ⚠️ 代码风格约定  
- ⚠️ 基于 ADR，不得引入新约束  

---

### ADR-950_3：文档结构标准（Rule）

#### ADR-950_3_1 Guide 标准结构
```markdown
# 指南：标题

**目的**：[一句话说明]  
**相关 ADR**：[ADR-XXXX 链接]  
**前置条件**：[需要了解的内容]  
**适用场景**：[何时使用本指南]
```

---

## Enforcement（执法模型）

| 规则编号        | 执行级 | 执法方式                                       | Decision 映射      |
| ----------- | --- | ------------------------------------------ | ---------------- |
| ADR-950_1_1 | L1  | 自动检测非裁决性文档中出现 ADR 规则定义                     | §ADR-950_1_1     |
| ADR-950_1_2 | L1  | 自动检查文档层级引用是否遵循 ADR→Guide→FAQ→Case→Standard | §ADR-950_1_2     |
| ADR-950_2_1 | L1  | Guide / FAQ / Case / Standard 与 ADR 分离边界检测 | §ADR-950_2_1~2_5 |
| ADR-950_3_1 | L1  | Guide 文档结构模板检测                             | §ADR-950_3_1     |

* **L1（阻断级）**：非裁决性文档违反分离边界，CI 阻断
* 测试类建议：`ADR_950_Architecture_Tests.cs`
* 测试方法数量：7 个（对应 Rule/Clause）

---

## Non-Goals（明确不管什么）

* 文档内容质量
* 文本风格与排版
* 单元/集成测试
* 文档覆盖率

---

## Prohibited（禁止行为）

* 非裁决性文档定义新 ADR 规则
* Standard 引入未授权约束
* Guide / FAQ / Case 涉及裁决性决策
* 文档结构不符合模板

---

## Relationships（关系声明）

**Depends On**：

* [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)

**Depended By**：

* [ADR-951：案例库管理规范](ADR-951-case-repository-management.md)
* [ADR-952：工程标准与 ADR 分离边界](ADR-952-engineering-standard-adr-boundary.md)
* [ADR-960：Onboarding 文档治理规范](ADR-960-onboarding-documentation-governance.md)

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：

* [ADR-910：README 编写与维护治理规范](ADR-910-readme-governance-constitution.md)

---

## References（非裁决性参考）

### 模板

* [Guide 模板](../../templates/guide-template.md)
* [FAQ 模板](../../templates/faq-template.md)
* [Case 模板](../../templates/case-template.md)
* [Standard 模板](../../templates/standard-template.md)

### 相关文档

* [文档类型决策树](../../guides/document-type-decision-tree.md)
* [审计清单](../../templates/documentation-audit-checklist.md)

---

## History（版本历史）

| 版本  | 日期         | 说明                | 修订人   |
| --- | ---------- | ----------------- | ----- |
| 1.0 | 2026-01-29 | 初始版本              | 架构委员会 |
| 1.1 | 2026-01-30 | 调整结构标准，增加 L1 执法映射 | 架构委员会 |
| 1.2 | 2026-02-06 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
