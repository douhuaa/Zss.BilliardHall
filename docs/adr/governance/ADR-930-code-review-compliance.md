---
adr: ADR-930
title: "代码审查与 ADR 合规自检流程"
status: Final
level: Governance
version: "1.0"
deciders: "Architecture Board"
date: 2026-01-27
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-930：代码审查与 ADR 合规自检流程

> ⚖️ **本 ADR 是所有 PR 和代码审查流程的唯一裁决源，定义 ADR 合规性检查的执行标准。**

**状态**：✅ Final（裁决型ADR）  
## Focus（聚焦内容）

- PR 必填信息和变更类型声明
- ADR 相关 PR 的自检要求
- 架构测试失败的处理规则
- 责任人审查要求
- 架构破例的标注和记录

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------|------------------------------------------------|---------------------------|
| PR         | Pull Request，代码变更的审查单元                         | Pull Request              |
| 变更类型       | PR 的分类标签（feat/fix/docs/refactor/test/chore）     | Change Type               |
| ADR 相关 PR  | 涉及架构约束或 ADR 修改的 PR                             | ADR-Related PR            |
| Copilot 自检 | 使用 code_review 工具进行架构合规性自检                     | Copilot Self-Check        |
| 架构测试失败     | ArchitectureTests 执行失败                          | Architecture Test Failure |
| 架构破例       | 经批准的临时性架构约束违反                                  | Architecture Exception    |
| 责任人        | 具有审查权限的 Tech Lead/模块负责人/架构师                   | Reviewer                  |

---

---

## Decision（裁决）

### PR 必须填写变更类型和影响范围（ADR-930.1）

**规则**：

所有 Pull Request **必须**在描述中明确标注：

- ✅ 变更类型：feat/fix/docs/refactor/test/chore
- ✅ 影响模块：明确列出受影响的模块或层
- ✅ ADR 相关性：如变更涉及 ADR，必须标注 ADR 编号
- ❌ 禁止空白 PR 描述

**PR 模板强制字段**：
```markdown

---

## Enforcement（执法模型）

所有规则通过以下方式强制验证：

- **PR 模板验证**：GitHub PR 模板强制要求填写变更类型和影响范围
- **Branch Protection**：GitHub 分支保护规则要求至少一名责任人批准
- **CI 提醒**：ADR 相关变更时提醒执行 Copilot 自检
- **人工审查**：Code Review 检查架构测试失败说明和破例标注

**有一项违规视为流程违规，需修正后才可合并。**

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
- [ADR-0000：架构测试与 CI 治理宪法](./ADR-0000-architecture-tests.md) - 代码审查流程基于测试和 CI 机制
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 文档变更需遵循文档治理规则
- [ADR-900：ADR 新增与修订流程](./ADR-900-adr-process.md) - ADR 相关 PR 需遵循 ADR 流程

**被依赖（Depended By）**：
- 无（代码审查规则是终端流程规则）

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0007：Agent 行为与权限宪法](../constitutional/ADR-0007-agent-behavior-permissions-constitution.md) - Copilot 在审查中的角色

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 待补充 | 初始版本 |
