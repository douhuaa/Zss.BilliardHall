---
adr: ADR-952
title: "工程标准与 ADR 分离边界"
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

# ADR-952：工程标准与 ADR 分离边界

> ⚖️ **本 ADR 定义工程标准（Engineering Standard）的定位、权限边界以及与 ADR 的关系规则。**

**状态**：✅ Accepted

## Focus（聚焦内容）

- 工程标准层级定义
- 工程标准与 ADR 的关系规则
- 工程标准权限边界
- 冲突裁决机制

---

## Glossary（术语表）

| 术语   | 定义                               | 英文对照                 |
|------|----------------------------------|----------------------|
| 工程标准 | 细化 ADR 实施细节的技术规范                 | Engineering Standard |
| 层级关系 | ADR、Standard、Best Practice 的权威层级 | Hierarchy            |
| 权限边界 | Standard 可以定义和不可以定义的内容           | Authority Boundary   |
| 冲突裁决 | Standard 与 ADR 冲突时的处理规则          | Conflict Resolution  |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
>
> 🔒 **统一铁律**：
>
> ADR-952 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-952_<Rule>_<Clause>
> ```

---

### ADR-952.1：层级定义与权威关系（Rule）

#### ADR-952.1.1 三层架构定义

- 三层架构：

```

L1 ADR（架构约束，最高权威）
↓ 基于
L2 Engineering Standard（实施标准，有限权威）
↓ 推荐
L3 Best Practice（推荐做法，无强制力）

```

#### ADR-952.1.2 层级定义表格

- 层级定义：

| 层级 | 名称            | 定义范围           | 强制力   | 示例                         |
|----|---------------|----------------|-------|----------------------------|
| L1 | ADR           | 架构约束、术语定义、测试要求 | ✅ 强制  | "模块禁止直接引用"                 |
| L2 | Standard      | 工具配置、代码风格、命名细节 | ⚠️ 有限 | "Handler 命名必须以 Handler 结尾" |
| L3 | Best Practice | 推荐做法、优化建议      | ❌ 无   | "建议使用异步编程"                 |

#### ADR-952.1.3 核心原则

- 核心原则：
  - ADR 定义必须/禁止
  - Standard 细化如何执行，但不得引入新约束
  - Best Practice 提供建议，不强制

#### ADR-952.1.4 判定

- 判定：
  - ❌ Standard 引入 ADR 未定义约束
  - ❌ Best Practice 使用强制性语言
  - ✅ Standard 细化 ADR，Best Practice 提供建议

---

### ADR-952.2：工程标准必须基于 ADR（Rule）

#### ADR-952.2.1 明确声明基于的 ADR

- 明确声明基于的 ADR

#### ADR-952.2.2 仅细化 ADR 的实施细节

- 仅细化 ADR 的实施细节

#### ADR-952.2.3 不得引入 ADR 未授权的新约束

- 不得引入 ADR 未授权的新约束

#### ADR-952.2.4 Standard 文档必需章节

- Standard 文档必需章节：

```markdown
# 工程标准：标题

**基于 ADR**：[ADR-XXXX](链接) - 说明依据哪个 ADR  
**类型**：配置标准/命名标准/工具标准  
**强制级别**：必须/应当/建议
```

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
>
> 下表展示了 ADR-952 各条款（Clause）的执法方式及执行级别。

| 规则编号            | 执行级 | 执法方式                           | Decision 映射  |
|-----------------|-----|--------------------------------|--------------|
| **ADR-952.1.1** | L1  | 自动扫描 Standard 与 ADR 层级关系       | §ADR-952.1.1 |
| **ADR-952.1.2** | L1  | 自动扫描 Standard 与 ADR 层级关系       | §ADR-952.1.2 |
| **ADR-952.1.3** | L1  | 自动扫描 Standard 与 ADR 层级关系       | §ADR-952.1.3 |
| **ADR-952.1.4** | L1  | 自动扫描 Standard 与 ADR 层级关系       | §ADR-952.1.4 |
| **ADR-952.2.1** | L1  | 检查 Standard 是否明确基于 ADR 且未引入新约束 | §ADR-952.2.1 |
| **ADR-952.2.2** | L1  | 检查 Standard 是否明确基于 ADR 且未引入新约束 | §ADR-952.2.2 |
| **ADR-952.2.3** | L1  | 检查 Standard 是否明确基于 ADR 且未引入新约束 | §ADR-952.2.3 |
| **ADR-952.2.4** | L1  | 检查 Standard 是否明确基于 ADR 且未引入新约束 | §ADR-952.2.4 |

### 执行级别说明

- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决

---

## Non-Goals（明确不管什么）

* Best Practice 内容是否被采纳
* Standard 文档排版、样式
* 工具本身实现的好坏

---

## Prohibited（禁止行为）

* Standard 引入 ADR 未定义的新约束
* Best Practice 使用强制性语言
* Standard 未明确声明基于的 ADR

---

## Relationships（关系声明）

**Depends On**：

* [ADR-950：指南与 FAQ 文档治理规范](ADR-950-guide-faq-documentation-governance.md)
* [ADR-900：ADR 新增与修订流程](ADR-900-architecture-tests.md)

**Depended By**：

* 所有 Standard 依赖管理和引用

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：

* [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md)

---

## References（非裁决性参考）

### 模板

* [Standard 模板](../../templates/standard-template.md)

### 相关文档

* [工程标准索引](../../engineering-standards/README.md)
* [Standard vs ADR 决策树](../../guides/standard-vs-adr-decision-tree.md)

---

## History（版本历史）

| 版本  | 日期         | 说明                                    | 修订人                |
|-----|------------|---------------------------------------|--------------------|
| 2.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。创建 ADR_952_1/2_Architecture_Tests，实现所有 2 个 Rule（层级定义与权威关系、工程标准必须基于 ADR）和 8 个 Clause 的测试用例。验证工程标准与 ADR 的分离边界。 | Copilot Agent |
| 1.0 | 2026-01-29 | 初始版本                                  | 架构委员会              |
