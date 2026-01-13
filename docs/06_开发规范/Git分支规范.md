---
title: "Git 分支规范"
description: "GitHub Flow 分支模型和中文提交信息规范"
section: "6.4"
version: "1.0.0"
author: "技术负责人"
maintainer: "开发工程师"
created: "2024-01-01"
updated: "2025-09-24"
category: "开发规范"
level: "必读"
audience: ["全体开发人员"]
keywords: ["Git分支", "GitHub Flow", "提交规范", "中文提交", "分支管理", "版本控制"]
tags: ["git", "branching", "commit-standards", "version-control"]
status: "完成"
dependencies: []
related_docs: ["06_开发规范/CodeReview流程.md", "12_版本与变更管理/README.md"]
reading_time: "15分钟"
difficulty: "初级"
---

# 6.4 Git 分支规范

<!-- Breadcrumb Navigation -->
**导航路径**: [🏠 项目文档首页](../自助台球系统项目文档.md) > [📝 开发规范](README.md) > 🌿 Git 分支规范

<!-- Keywords for Search -->
**关键词**: `Git分支` `GitHub Flow` `提交规范` `中文提交` `分支管理` `版本控制`

> **变更说明**  
> 分支模型由 Git Flow 切换为 GitHub Flow，以提升持续集成效率并简化分支管理，主分支始终保持可部署状态。

- **变更原因**：GitHub Flow 更简洁，适合持续集成和频繁发布，减少分支管理复杂度。
- **影响**：开发流程更为敏捷，分支数量减少，发布流程简化。

## 分支模型

本项目采用 **GitHub Flow** 分支模型，适合快速迭代和持续部署。

> ⚠️ 不再使用 `develop` 分支：为保持流程简单（单主线可部署），所有功能分支均直接自 `main` 派生；若发现遗留文档/脚本仍引用 `develop`，请在同一 PR 一并更正。

### 主要分支

#### 1. main（主分支）

- **用途**：生产环境代码，始终保持稳定可部署状态。
- **保护规则**：
  - 禁止直接 push。
  - 必须通过 Pull Request 合并。
  - 需要至少 1 人代码审查。
  - 必须通过 CI/CD 检查。
  - 必须是最新代码（不允许过时的分支合并）。
- **命名规范**：

  ```text
  feature/用户管理模块
  feature/支付系统集成
  feature/台球桌预约功能
  ```

- **生命周期**：从 main 创建，完成后通过 Pull Request 合并回 main。
- **删除规则**：合并后删除分支。

> **发布准备和热修复说明**  
> 在 GitHub Flow 模型下，发布准备可通过创建 `feature/发布准备` 分支进行，完成后合并至 main 并打 tag 发布。  
> 热修复场景可直接从 main 创建 `feature/热修复-xxx` 分支，修复后通过 Pull Request 合并至 main 并立即部署，无需单独的 hotfix 分支。

### 分支命名规范

| 场景 | 前缀 | 示例 | 说明 |
|------|------|------|------|
| 功能 | `feature/` | `feature/会员积分系统` | 单一业务能力或用例；避免“一锅端”式超大分支 |
| 缺陷 | `fix/` | `fix/支付回调验签` | 线上或测试环境发现的缺陷修复 |
| 性能 | `perf/` | `perf/桌状态轮询优化` | 明确性能目标（延迟/吞吐） |
| 重构 | `refactor/` | `refactor/计费策略抽象` | 不改变外部行为的结构性调整 |
| 文档 | `docs/` | `docs/补充日志规范` | 纯文档或元信息变更 |
| 安全 | `security/` | `security/移除弱加密算法` | 安全加固/修复 |
| 依赖 | `deps/` | `deps/upgrade-abp-9.0` | 升级/替换依赖，需附兼容说明 |
| 临时探索 | `spike/` | `spike/miniapp-sdk-eval` | 不保证合并；结论需在 PR 描述标注 |
| 发布准备 | `release/` (可选) | `release/1.2.0` | 若需要集中验证，可短期存在；合并后打 Tag |

命名要求：

1. 使用语义清晰的简短中文或中英混合，不使用拼音首字母。
2. 避免过度层级（`feature/a/b/c`），必要子域可使用中划线：`feature/计费-折扣策略`。
3. 单个分支存活期尽量 < 5 天；超过需考虑拆分或阶段性合并。

### 分支同步与 Rebase 策略

保持分支最新，减少合并冲突：

```bash
# 同步 main（始终先更新本地主分支）
git checkout main
git pull --ff-only origin main

# 回到功能分支并 Rebase（推荐）
git checkout feature/会员积分系统
git fetch origin main
git rebase origin/main

# 若发生冲突：按文件解决后继续
git add <file>
git rebase --continue

# 推送（rebase 后需强制推送当前分支）
git push -f origin feature/会员积分系统
```

原则：

- 首选 `rebase` 保持线性历史；公共分支（main）禁止 rebase。
- 若冲突多次反复，考虑“早合并拆分”——先合并可独立交付的子功能。
- 严禁通过 “把 main 合入分支 + 再合 PR” 制造复杂分叉（会增加审查认知成本）。

### Draft PR（草稿 PR）使用

场景：需要早期反馈 / 架构走查 / 跨模块协调。

- 创建 PR 时标记为 Draft，标题前可加 `WIP:`。
- 草稿阶段不要求完整测试，但需：
  - 目录结构基本稳定；
  - 关键接口/DTO 初稿到位；
  - 标记待定点：使用 `// TODO:` 并附意图说明。
- 转为 Ready for Review 前：通过格式化、解决编译错误、基础单元测试。

### Tag 与发布

- 合并发布相关功能后：`git tag -a v1.2.0 -m "Release 1.2.0" && git push origin v1.2.0`。
- Tag 命名遵循 `v<主>.<次>.<修>`（语义化版本）。
- 重要发布需在 `CHANGELOG` 中列出：新增 / 变更 / 修复 / 废弃 / 移除 / 安全。
- 参考：`12_版本与变更管理/变更日志.md`（如未存在请创建）。

### 自动关闭 Issue 关键字

在提交信息或 PR 描述中使用以下关键字 + Issue 号：

| 关键字 | 行为 |
|--------|------|
| `Closes #123` / `Close` / `Closed` | 合并到默认分支后自动关闭 Issue |
| `Fixes #123` / `Fix` / `Fixed` | 同上，强调缺陷修复 |
| `Resolves #123` / `Resolve` / `Resolved` | 同上，强调需求交付 |
| `Refs #123` | 不自动关闭，仅建立引用 |

建议：一条提交聚焦单一 Issue；多 Issue 时在 PR 描述中集中列出。

## 提交信息规范

### 提交信息格式

```text
<类型>(<范围>): <简短描述>

<详细描述>（可选）

<关闭的Issue>（可选）
```text

### 提交类型（统一清单）

| 类型(中文) | 英文类型 | 说明 | 示例 |
|------------|----------|------|------|
| 功能 | feat | 新增用户可见功能 | `feat(积分): 添加积分结算接口` |
| 修复 | fix | 缺陷修复 | `fix(支付): 修复回调验签逻辑` |
| 文档 | docs | 仅文档 / 注释 | `docs: 补充分层约束示例` |
| 样式 | style | 格式 / 空白 / 风格（无逻辑变更） | `style: 调整 using 排序` |
| 重构 | refactor | 结构性调整（无行为变化） | `refactor(计费): 拆分费用计算策略` |
| 性能 | perf | 性能优化（含内存、延迟） | `perf(统计): 优化聚合查询` |
| 测试 | test | 新增/调整测试 | `test(会员): 增加积分过期测试` |
| 构建 | build | 构建脚本 / 工具链 / 打包 | `build: 新增 docker buildx 配置` |
| CI/CD | ci | CI/CD 流水线配置 | `ci: 引入缓存加速 restore` |
| 依赖 | deps | 第三方包升级/替换 | `deps: 升级 abp 到 9.0` |
| 安全 | security | 安全修复 / 加固 | `security(auth): 移除弱密码策略` |
| 回滚 | revert | 撤销先前提交 | `revert: revert feat(积分) 因性能回退` |
| 杂项 | chore | 其他杂项（不影响源码逻辑） | `chore: 更新 .gitignore` |

说明：
- 优先使用最贴近语义的类型；`chore` 仅兜底，不做“一切杂项”收纳箱。
- 与 PR 模板“变更类型”多选框保持一致；若新增类型需同步两个文档与模板。
- 重大不兼容修改需在提交体或 PR 描述添加 `BREAKING CHANGE:` 块并在变更日志中记录。

### 提交信息示例

#### 良好的提交信息

```bash
# 新功能
git commit -m "feat(台球桌): 添加台球桌实时状态监控

- 实现台球桌状态实时更新
- 添加WebSocket推送机制
- 完善异常处理逻辑

Closes #123"

# Bug修复
git commit -m "fix(支付): 修复微信支付回调验签失败问题

原因：回调参数顺序不正确导致验签失败
解决：按照微信API文档重新排序参数

Fixes #456"

# 数据库迁移
git commit -m "feat(数据库): 添加会员管理相关表结构

- 新增Members表
- 新增MembershipPlans表
- 添加相关索引和约束
- 更新EF Core迁移文件"
```

#### 避免的提交信息

```bash
# ❌ 过于简单
git commit -m "修复bug"
git commit -m "更新"
git commit -m "WIP"

# ❌ 使用英文（项目要求中文）
git commit -m "fix payment issue"
git commit -m "add new feature"
```

## 工作流程

### 功能开发流程

```bash
# 1. 从main创建功能分支
git checkout main
git pull origin main
git checkout -b feature/会员积分系统

# 2. 开发过程中定期提交
git add .
git commit -m "feat(积分): 实现积分计算核心逻辑"

# 3. 推送到远程仓库
git push -u origin feature/会员积分系统

# 4. 创建Pull Request到main分支
# 5. 代码审查通过后合并
# 6. 删除功能分支
git branch -d feature/会员积分系统
git push origin --delete feature/会员积分系统
```

## Pull Request 规范

### PR 标题格式

优先采用与首个（或主导）提交一致的 Conventional Commit 语义：

```text
<type>(<scope>): <中文简述>
```

示例：

```text
feat(积分): 支持积分结算与过期回收
fix(支付): 修复签名大小写导致的验签失败
refactor(计费): 拆分桌计费策略为独立服务
```

允许在评审阶段为了可读性临时采用标签形式：`[功能] 积分结算`，合并前由作者统一规整为标准格式。

### PR 描述统一模板

本节仅保留最小必要字段；完整勾选/检查清单在 `.github/pull_request_template.md`，创建 PR 时会自动加载。

最少字段：

```markdown
## 变更概述
一句话 + 必要背景

## 关联 Issue
Closes #123

## 评审关注点
列出 2~5 个最希望 Reviewer 聚焦的点
```

> 完整字段（测试、风险、兼容性、回滚、部署）请使用自动模板或参考 Code Review 流程文档。

> 旧版内嵌大模板已移除，避免与实际 PR 模板重复维护。

## 常用Git命令

```bash
# 查看分支情况
git branch -a
git branch -r

# 清理本地分支
git branch --merged | grep -v "\*\|main" | xargs -n 1 git branch -d

# 更新本地分支列表
git fetch --prune origin

# 查看提交历史
git log --oneline --graph --decorate --boundary

# 查看某个文件的变更历史
git log --follow --patch -- 文件名
```

---

## 📚 相关文档

### 同级文档

- [6.1 代码风格](代码风格.md)
- [6.2 分层约束](分层约束.md)
- [6.3 日志规范](日志规范.md)
- [6.5 Code Review流程](CodeReview流程.md)

### 返回上级

- [🔙 开发规范总览](README.md)
- [🏠 项目文档首页](../自助台球系统项目文档.md)

### 相关章节

- [5.5 数据迁移方案](../05_数据库设计/数据迁移方案.md)
- [12. 版本与变更管理](../12_版本与变更管理/README.md)
