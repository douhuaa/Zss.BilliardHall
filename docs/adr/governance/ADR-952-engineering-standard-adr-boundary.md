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
**版本**：1.0
**级别**：治理层 / 架构元规则  
**适用范围**：所有工程标准文档（docs/engineering-standards/）  
**生效时间**：即刻

---

## Focus（聚焦内容）

- 工程标准的层级定义
- 工程标准与 ADR 的关系规则
- 工程标准的权限边界
- 冲突裁决机制

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 工程标准 | 细化 ADR 实施细节的技术规范 | Engineering Standard |
| 层级关系 | ADR、Standard、Best Practice 的权威层级 | Hierarchy |
| 权限边界 | Standard 可以定义和不可以定义的内容 | Authority Boundary |
| 冲突裁决 | Standard 与 ADR 冲突时的处理规则 | Conflict Resolution |

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

## ADR 授权范围

引用 ADR-XXXX 的相关规则：
> [引用 ADR 原文]

本标准细化以下方面：
- [细化点 1]
- [细化点 2]

---

## 实施标准

[具体标准内容]
```

**允许细化的内容**：
- ✅ 配置文件格式
- ✅ 工具版本要求
- ✅ 命名详细规则
- ✅ 代码风格约定
- ✅ 目录组织细节

**禁止定义的内容**：
- ❌ 新的架构约束
- ❌ ADR 未提及的禁止项
- ❌ 与 ADR 冲突的规则
- ❌ 新的术语定义

**精神条款（防止隐性立法）**：

如果一个 Standard 的规则让读者在**不查阅 ADR 的情况下**，也能得出"必须/禁止"的结论，则该 Standard 已越权。

**越权识别测试**：
1. 遮住 Standard 中的"基于 ADR"引用
2. 让不熟悉 ADR 的人阅读 Standard
3. 询问：这是建议还是强制要求？
4. 如果答案是"强制"且无法从 ADR 推导，则越权

**示例**：
```markdown
❌ 越权示例：
Standard：所有 API 必须返回统一的错误格式 ErrorResponse

→ 读者会认为这是强制要求
→ 但 ADR 未定义此约束
→ Standard 在伪装成架构决策

✅ 正确细化：
ADR-XXX：API 必须返回结构化错误
Standard：基于 ADR-XXX，错误格式实现为 ErrorResponse 类

→ 读者知道约束来自 ADR
→ Standard 仅细化实现方式
```

**判定**：
- ❌ Standard 未声明基于的 ADR
- ❌ Standard 引入新约束
- ❌ Standard 规则无法从 ADR 推导
- ✅ Standard 明确基于 ADR 并仅细化实施

---

### 冲突时以 ADR 为准（ADR-952.3）

**规则**：

**冲突裁决**：
- ADR 与 Standard 冲突时，以 ADR 为准
- Standard 必须更新以符合 ADR
- 冲突期间，ADR 规则有效

**冲突识别**：
1. **直接冲突**：Standard 要求与 ADR 相反
2. **扩展冲突**：Standard 禁止 ADR 允许的行为
3. **隐含冲突**：Standard 的实施细节违反 ADR 精神

**冲突处理流程**：
1. 发现冲突时，创建 Issue 标记 `adr-standard-conflict`
2. 暂停执行 Standard 的冲突部分
3. 修改 Standard 以符合 ADR
4. 或提出 ADR 修订请求（走 RFC 流程）

**示例**：
```markdown
❌ 冲突示例：
- ADR-0001：模块可通过契约查询数据
- Standard：禁止跨模块查询
→ 以 ADR-0001 为准，Standard 必须修正

✅ 正确细化：
- ADR-0001：模块可通过契约查询数据
- Standard：契约查询必须使用 IContractQuery<T> 接口
→ Standard 细化了实施方式，不冲突
```

**判定**：
- ❌ 冲突未识别或未处理
- ❌ Standard 试图覆盖 ADR
- ✅ 冲突及时识别并以 ADR 为准

---

### Standard 可提升为 ADR（ADR-952.4）

**规则**：

**提升条件**：
Standard 满足以下条件时，应考虑提升为 ADR：

1. **影响架构决策**：Standard 的规则影响系统整体设计
2. **需要测试执行**：Standard 需要架构测试强制执行
3. **跨模块适用**：Standard 影响多个模块或层级
4. **不可协商**：Standard 的规则不允许例外

**提升流程**：
1. 创建 RFC，说明提升理由
2. 走 ADR-900 正式流程
3. 创建正式 ADR
4. 更新或废弃原 Standard

**示例**：
```markdown
Standard → ADR 提升案例：

原 Standard："Repository 接口必须位于 Domain 层"
→ 影响模块设计，需要测试执行
→ 提升为 ADR-123

更新后：
ADR-123 定义规则
Standard 引用 ADR-123 并细化实施
```

**判定**：
- ❌ 应提升但未提升的 Standard
- ✅ Standard 及时评估并提升为 ADR

---

## 执法模型（Enforcement）

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-952.1 | L2 | Manual Review |
| ADR-952.2 | L2 | `scripts/verify-standard-adr-reference.sh` |
| ADR-952.3 | L2 | Issue Tracking |
| ADR-952.4 | L2 | Quarterly Review |

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 历史遗留 Standard 需要时间更新

### 破例要求

每个破例 **必须**：
- 记录在 `ARCH-VIOLATIONS.md`
- 指定失效日期（不超过 3 个月）
- 给出更新计划

---

## 变更政策（Change Policy）

### 变更规则

- **修改层级定义**：需架构委员会批准
- **修改冲突裁决规则**：需架构委员会批准
- **修改提升条件**：Tech Lead 可批准

---

## Non-Goals（明确不管什么）

本 ADR **不负责**：
- Standard 内容的技术准确性
- Standard 的具体实施细节
- Best Practice 的管理

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-950：指南与 FAQ 文档治理宪法](ADR-950-guide-faq-documentation-governance.md) - 基于 Standard 类型定义
- [ADR-900：ADR 新增与修订流程](ADR-900-adr-process.md) - Standard 提升为 ADR 流程

**被依赖（Depended By）**：无

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档分级

---

## References（非裁决性参考）

### 模板

- [Standard 模板](../../templates/standard-template.md)

### 相关文档

- [工程标准索引](../../engineering-standards/README.md)
- [Standard vs ADR 决策树](../../guides/standard-vs-adr-decision-tree.md)

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初始版本，定义工程标准与 ADR 边界 | GitHub Copilot |




---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

## Non-Goals（明确不管什么）


本 ADR 明确不涉及以下内容：

- 待补充


---

## Enforcement（执法模型）


### 执行方式

待补充...


---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
