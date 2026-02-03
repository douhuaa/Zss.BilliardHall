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

- **代码审查工具的选择**：不规定使用 GitHub PR Review、GitLab MR 还是其他代码审查工具
- **代码审查的详细检查清单**：不提供逐项的代码质量检查项（如命名、注释等）
- **代码审查的时效性要求**：不规定审查必须在多长时间内完成
- **代码审查的人员资格**：不规定谁有资格进行代码审查或需要什么级别审批
- **代码审查的沟通技巧**：不涉及如何给出建设性反馈等软技能
- **代码审查的会议流程**：不规定是否需要同步会议讨论代码
- **非架构相关的代码质量**：不涉及性能优化、安全漏洞等非架构约束的审查
- **代码审查的工作量统计**：不建立代码审查工作量的度量和考核机制

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 审查绕过
- ❌ **禁止未经代码审查直接合并**：所有包含代码变更的 PR 必须经过至少一次审查
- ❌ **禁止自行批准自己的 PR**：代码作者不得批准自己的变更
- ❌ **禁止以"紧急修复"为由跳过架构审查**：紧急修复也必须符合架构约束

### 审查质量违规
- ❌ **禁止未运行架构测试就批准 PR**：审查者必须确认架构测试通过
- ❌ **禁止忽视明显的架构违规**：审查者发现架构违规必须拒绝批准
- ❌ **禁止"看起来没问题"式的敷衍审查**：必须实际验证代码是否符合 ADR

### 记录违规
- ❌ **禁止不记录审查决策依据**：重要的审查意见必须引用具体的 ADR 编号
- ❌ **禁止口头批准代码变更**：所有批准必须在 PR 系统中正式记录
- ❌ **禁止修改代码后不重新审查**：代码有实质性修改后必须重新请求审查


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 代码审查流程基于测试和 CI 机制
- [ADR-008：文档编写与维护宪法](../constitutional/ADR-008-documentation-governance-constitution.md) - 文档变更需遵循文档治理规则
- [ADR-900：ADR 新增与修订流程](./ADR-900-architecture-tests.md) - ADR 相关 PR 需遵循 ADR 流程

**被依赖（Depended By）**：
- 无（代码审查规则是终端流程规则）

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-007：Agent 行为与权限宪法](../constitutional/ADR-007-agent-behavior-permissions-constitution.md) - Copilot 在审查中的角色

---

---

## References（非裁决性参考）

**相关外部资源**：
- [Google Code Review Guidelines](https://google.github.io/eng-practices/review/) - Google 工程实践中的代码审查指南
- [GitHub Pull Request Best Practices](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/getting-started/best-practices-for-pull-requests) - GitHub PR 最佳实践
- [The Art of Code Review](https://www.atlassian.com/agile/software-development/code-reviews) - Atlassian 关于代码审查的指南

**相关内部文档**：
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 审查中的测试验证要求
- [ADR-007：Agent 行为与权限宪法](../constitutional/ADR-007-agent-behavior-permissions-constitution.md) - Copilot 在审查中的辅助角色
- [ADR-900：ADR 新增与修订流程](./ADR-900-architecture-tests.md) - ADR 相关变更的审查流程
- [ADR-905：执行级别分类](./ADR-905-enforcement-level-classification.md) - 不同级别违规的审查处理


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
