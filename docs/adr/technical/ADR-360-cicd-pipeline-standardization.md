---
adr: ADR-360
title: "CI/CD Pipeline 流程标准化"
status: Final
level: Technical
version: "2.0"
deciders: "Architecture Board"
date: 2026-01-26
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-360：CI/CD Pipeline 流程标准化

**影响范围**：所有 CI/CD 流程、PR 合并流程  
## Focus（聚焦内容）

- PR 合并前的强制性架构测试约束
- PR 标题格式规范与自动化验证
- 主分支保护规则
- CI 失败信息清晰性要求
- 构建产物版本信息标准
- CI 失败责任归属机制

---

---

## Glossary（术语表）


| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 待补充 | 待补充 | TBD |


---

---

## Decision（裁决）

### ADR-360.1：PR 必须通过架构测试【必须架构测试覆盖】

Pull Request **必须**通过所有架构测试才能合并。

**规则**：
- 架构测试**必须**在 CI 中自动运行
- 架构测试失败**必须**阻止 PR 合并
- **禁止**跳过架构测试
- **禁止**在 CI 中标记架构测试为可选

**判定**：
- ❌ CI 配置允许架构测试失败仍可合并
- ❌ PR 绕过架构测试检查
- ✅ 架构测试全部通过才允许合并

---

### ADR-360.2：PR 标题必须符合 Conventional Commits【必须架构测试覆盖】

所有 PR 标题**必须**以 Conventional Commits 类型前缀开头。

**规则**：
- **必须**使用规范前缀：`feat:`、`fix:`、`docs:`、`refactor:`、`test:`、`chore:`
- **禁止**无前缀
- **禁止**自定义前缀

**判定**：
- ❌ PR 标题为 "Update README"（无前缀）
- ❌ PR 标题为 "feature: Add new API"（错误前缀）
- ✅ PR 标题为 "feat: Add new API"

**验证**：CI 自动检查 PR 标题格式，不符合格式的 PR 无法合并。

---

### ADR-360.3：main 分支必须受保护【必须架构测试覆盖】

`main` 分支**必须**启用分支保护规则。

**规则**：
- **必须**禁止直接推送
- **必须**要求 PR 审查
- **必须**要求状态检查通过
- **必须**要求分支为最新状态

**判定**：
- ❌ main 分支允许直接推送
- ❌ main 分支未启用状态检查
- ✅ main 分支完整保护规则

---

### ADR-360.4：CI 失败必须提供可操作错误信息【必须架构测试覆盖】

CI 失败时**必须**提供清晰、可操作的错误信息。

**规则**：
- **必须**明确指出失败的步骤
- **必须**提供失败原因和修复建议
- **必须**包含相关文档链接（如 ADR 编号）
- **禁止**仅输出堆栈跟踪无说明

**判定**：
- ❌ CI 失败仅显示 "Build failed" 无详情
- ❌ CI 失败输出堆栈跟踪无修复指引
- ✅ CI 失败包含失败原因、修复建议、文档链接

---

### ADR-360.5：构建产物必须包含版本信息【必须架构测试覆盖】

所有构建产物**必须**包含完整的版本和构建信息。

**规则**：
- **必须**包含 Git commit SHA
- **必须**包含构建时间
- **必须**包含分支名称
- **必须**包含版本号（如适用）

**判定**：
- ❌ 构建产物缺少 commit SHA
- ❌ 构建产物无法追溯源代码
- ✅ 构建产物包含全部版本信息

---

### ADR-360.6：CI 失败必须明确责任归属【必须架构测试覆盖】

CI 失败时**必须**在失败消息中明确归属责任类别。

**规则**：
- **必须**区分失败类型：架构测试、功能测试、构建失败、格式检查
- **必须**标明责任归属：PR 作者、DevOps 团队、平台团队
- **必须**提供修复指引链接

**失败分类**：
- **架构测试失败** → 架构规则违反（责任：PR 作者）
- **单元/集成测试失败** → 功能实现问题（责任：PR 作者）
- **构建失败** → 平台或流水线问题（责任：DevOps/Platform 团队）
- **格式/Lint 失败** → 代码风格问题（责任：PR 作者，自动修复）

**判定**：
- ❌ CI 失败仅显示 "Tests failed" 无分类
- ✅ CI 失败消息包含类别、责任人、修复指引

**推荐格式**：
```
❌ CI 失败类别：【架构测试】
责任归属：PR 作者
失败测试：ADR_001_Architecture_Tests.Modules_Must_Not_Cross_Reference
修复指引：docs/copilot/adr-001.prompts.md
```

---

---

## Enforcement（执法模型）

### 测试实现

**L1 自动执行**：
- GitHub Actions 强制架构测试检查
- CI Workflow 自动验证 PR 标题格式
- GitHub 分支保护规则强制执行
- 构建脚本验证版本信息注入

**L2 人工审查**：
- 人工审查 CI 配置完整性
- CI 失败消息模板验证
- 责任归属机制审查

**CI 阻断条件**：
- 架构测试失败 → CI 失败
- PR 标题格式错误 → CI 失败
- 构建产物缺少版本信息 → CI 失败

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
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - CI/CD 管道基于 CI 治理机制
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - CI/CD 术语遵循统一规范

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-930：代码审查合规](../governance/ADR-930-code-review-compliance.md) - 代码审查是 CI/CD 的一部分

---

---

## References（非裁决性参考）

### 相关 ADR
- ADR-900：架构测试与 CI 治理元规则
- ADR-930：代码审查与 ADR 合规自检流程

### 技术资源
- [Conventional Commits 规范](https://www.conventionalcommits.org/zh-hans/)
- [GitHub 分支保护规则](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches)

### 实践指导
- CI 配置示例参见 `.github/workflows/`
- 常见问题参见 `docs/copilot/adr-360.prompts.md`（待创建）

---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 1.0 | 2026-01-29 | 初始版本 |
