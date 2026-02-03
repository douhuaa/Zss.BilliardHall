---
adr: ADR-952
title: "工程标准与 ADR 分离边界"
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


# ADR-952：工程标准与 ADR 分离边界

> ⚖️ **本 ADR 定义工程标准（Engineering Standard）的定位、权限边界以及与 ADR 的关系规则。**

**状态**：✅ Accepted（已采纳）  
## Focus（聚焦内容）

- 工程标准的层级定义
- 工程标准与 ADR 的关系规则
- 工程标准的权限边界
- 冲突裁决机制

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 工程标准 | 细化 ADR 实施细节的技术规范 | Engineering Standard |
| 层级关系 | ADR、Standard、Best Practice 的权威层级 | Hierarchy |
| 权限边界 | Standard 可以定义和不可以定义的内容 | Authority Boundary |
| 冲突裁决 | Standard 与 ADR 冲突时的处理规则 | Conflict Resolution |

---

---

## Decision（裁决）

### 层级定义与权威关系（ADR-952.1）

**规则**：

**三层架构**：
```
L1 ADR（架构约束，最高权威）
 ↓ 基于
L2 Engineering Standard（实施标准，有限权威）
 ↓ 推荐
L3 Best Practice（推荐做法，无强制力）
```

**层级定义**：

| 层级 | 名称 | 定义范围 | 强制力 | 示例 |
|------|------|----------|--------|------|
| **L1** | ADR | 架构约束、术语定义、测试要求 | ✅ 强制 | "模块禁止直接引用" |
| **L2** | Standard | 工具配置、代码风格、命名细节 | ⚠️ 有限 | "Handler 命名后缀必须为 Handler" |
| **L3** | Best Practice | 推荐做法、优化建议 | ❌ 无 | "建议使用异步编程" |

**核心原则**：
> ADR 定义"必须/禁止"  
> Standard 细化"如何执行"但不得引入新约束  
> Best Practice 提供"建议"但不强制

**判定**：
- ❌ Standard 引入 ADR 未定义的约束
- ❌ Best Practice 使用强制性语言
- ✅ Standard 细化 ADR，Best Practice 提供建议

---

### 工程标准必须基于 ADR（ADR-952.2）

**规则**：

所有 Engineering Standard **必须**：
1. **明确声明基于的 ADR**
2. **仅细化 ADR 的实施细节**
3. **不得引入 ADR 未授权的新约束**

**Standard 文档必需章节**：
```markdown
# 工程标准：标题

**基于 ADR**：[ADR-XXXX](链接) - 说明依据哪个 ADR  
**类型**：配置标准/命名标准/工具标准  
**强制级别**：必须/应当/建议

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
- [ADR-950：指南与 FAQ 文档治理规范](ADR-950-guide-faq-documentation-governance.md) - 基于 Standard 类型定义
- [ADR-900：ADR 新增与修订流程](ADR-900-architecture-tests.md) - Standard 提升为 ADR 流程

**被依赖（Depended By）**：无

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档分级

---

---

## References（非裁决性参考）

### 模板

- [Standard 模板](../../templates/standard-template.md)

### 相关文档

- [工程标准索引](../../engineering-standards/README.md)
- [Standard vs ADR 决策树](../../guides/standard-vs-adr-decision-tree.md)

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
