---
adr: ADR-930
title: "代码审查与 ADR 合规自检流程"
status: Final
level: Governance
deciders: "Architecture Board"
date: 2026-01-26
version: "1.0"
maintainer: "Architecture Board"
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---

# ADR-930：代码审查与 ADR 合规自检流程

---
> ⚖️ **本 ADR 是所有 PR 和代码审查流程的唯一裁决源，定义 ADR 合规性检查的执行标准。**


---

## 聚焦内容（Focus）

- PR 必填信息和变更类型声明
- ADR 相关 PR 的自检要求
- 架构测试失败的处理规则
- 责任人审查要求
- 架构破例的标注和记录

---

## 术语表（Glossary）

| 术语         | 定义                                              | 英文对照                      |
|------------|------------------------------------------------|---------------------------|
| PR         | Pull Request，代码变更的审查单元                         | Pull Request              |
| 变更类型       | PR 的分类标签（feat/fix/docs/refactor/test/chore）     | Change Type               |
| ADR 相关 PR  | 涉及架构约束或 ADR 修改的 PR                             | ADR-Related PR            |
| Copilot 自检 | 使用 code_review 工具进行架构合规性自检                     | Copilot Self-Check        |
| 架构测试失败     | ArchitectureTests 执行失败                          | Architecture Test Failure |
| 架构破例       | 经批准的临时性架构约束违反                                  | Architecture Exception    |
| 责任人        | 具有审查权限的 Tech Lead/模块负责人/架构师                   | Reviewer                  |

---

## 决策（Decision）

### PR 必须填写变更类型和影响范围（ADR-930.1）

**规则**：

所有 Pull Request **必须**在描述中明确标注：

- ✅ 变更类型：feat/fix/docs/refactor/test/chore
- ✅ 影响模块：明确列出受影响的模块或层
- ✅ ADR 相关性：如变更涉及 ADR，必须标注 ADR 编号
- ❌ 禁止空白 PR 描述

**PR 模板强制字段**：
```markdown
## 变更类型
- [ ] feat: 新功能
- [ ] fix: Bug 修复
- [ ] docs: 文档更新
- [ ] refactor: 重构
- [ ] test: 测试
- [ ] chore: 构建/工具

## 影响范围
- 模块：[列出]
- ADR：[编号或无]
```

**判定**：
- ❌ 空白 PR 描述
- ❌ 未标注变更类型
- ❌ ADR 相关变更未标注 ADR 编号
- ✅ 完整的变更类型和影响范围声明

---

### ADR 相关 PR 必须通过 Copilot 自检（ADR-930.2）

**规则**：

涉及架构约束的 PR **必须**运行 Copilot 自检，并将结果附在 PR 中。

**触发条件**：
- 修改 ADR 文档
- 修改架构测试
- 新增模块或重大重构
- 变更核心基础设施

**自检要求**：
- ✅ 使用 `code_review` 工具进行自检
- ✅ 在 PR 描述中附上自检结果
- ✅ 解决所有标记为"必须修复"的问题
- ❌ 禁止跳过 Copilot 自检

**判定**：
- ❌ ADR 相关 PR 未执行自检
- ❌ 自检发现的"必须修复"问题未解决
- ✅ 完整的自检流程和结果记录

---

### 架构测试失败必须说明原因和计划（ADR-930.3）

**规则**：

如果 PR 中存在架构测试失败，**必须**在 PR 中说明原因和修复计划。

**说明要求**：
- ✅ 失败的具体测试和 ADR 编号
- ✅ 失败原因分析
- ✅ 修复计划或破例申请
- ❌ 禁止静默忽略测试失败

**允许的情况**：
- 测试本身有 Bug（需创建 Issue 修复测试）
- 合法的破例场景（需记录在 arch-violations.md）

**判定**：
- ❌ 测试失败未说明原因
- ❌ 静默忽略测试失败
- ✅ 完整的失败原因分析和修复计划

---

### 每个 PR 必须至少一名责任人审查（ADR-930.4）

**规则**：

Pull Request **必须**至少获得一名具有审查权限的责任人批准。

**责任人定义**：
- Tech Lead
- 模块负责人
- 架构师

**审查职责**：
- ✅ 验证变更符合相关 ADR
- ✅ 检查代码质量和安全性
- ✅ 确认测试覆盖充分
- ❌ 禁止机械式批准（无实质审查）

**判定**：
- ❌ 无责任人审查
- ❌ 机械式批准
- ✅ 责任人实质审查并批准

---

### 破例必须在 PR 中明确标注（ADR-930.5）

**规则**：

如果 PR 包含架构破例，**必须**在 PR 描述和代码中明确标注。

**标注要求**：
- ✅ PR 描述中说明破例的 ADR 和规则
- ✅ 代码中添加 `// ARCH-EXCEPTION: ADR-XXX.Y` 注释
- ✅ 更新 `docs/summaries/arch-violations.md`
- ❌ 禁止未声明的破例

**判定**：
- ❌ 破例未在 PR 中声明
- ❌ 代码中未标注破例注释
- ❌ arch-violations.md 未更新
- ✅ 完整的破例声明和记录

---

## 关系声明（Relationships）

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

## 快速参考表

| 约束编号       | 约束描述                | 测试方式             | 必须遵守 |
|------------|---------------------|------------------|------|
| ADR-930.1  | PR 必须填写变更类型和影响范围    | L1 - PR 模板强制验证 | ✅    |
| ADR-930.2  | ADR 相关 PR 必须 Copilot 自检 | L2 - 人工检查 + CI 提醒 | ✅    |
| ADR-930.3  | 架构测试失败必须说明原因        | L2 - 人工审查        | ✅    |
| ADR-930.4  | 每个 PR 必须责任人审查       | L1 - GitHub Branch Protection | ✅    |
| ADR-930.5  | 破例必须明确标注            | L2 - 人工审查 + 脚本检查 | ✅    |

---

## 必测/必拦架构测试（Enforcement）

所有规则通过以下方式强制验证：

- **PR 模板验证**：GitHub PR 模板强制要求填写变更类型和影响范围
- **Branch Protection**：GitHub 分支保护规则要求至少一名责任人批准
- **CI 提醒**：ADR 相关变更时提醒执行 Copilot 自检
- **人工审查**：Code Review 检查架构测试失败说明和破例标注

**有一项违规视为流程违规，需修正后才可合并。**

---

## 破例与归还（Exception）

### 允许破例的前提

流程规则的破例**仅在以下情况允许**：
- **紧急热修复**：生产环境重大故障
- **自动化工具故障**：Copilot 或 CI 系统不可用
- **小型文档修正**：纯文档拼写或格式修正

### 破例要求

紧急破例后**必须**：
- 在 24 小时内补充完整的审查流程
- 记录破例原因和时间
- 创建 Follow-up Issue 跟踪

---

## 变更政策（Change Policy）

- **ADR-930**（治理层）
  - 修改需架构师大多数同意
  - 需评估对团队工作流的影响
  - 必须提供过渡期方案

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- 代码风格检查（由 linter 负责）
- 具体的编码规范
- 性能优化建议
- 业务逻辑的正确性验证

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 2.0 | 2026-01-26 | 裁决型重构，移除冗余，补充决策章节 |
| 1.0 | 2026-01-24 | 初版，定义 PR 和代码审查流程 |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（详细示例、审查清单）请查阅：
- PR 模板参见 `.github/PULL_REQUEST_TEMPLATE.md`
- 审查清单参见 `docs/copilot/adr-0930.prompts.md`
