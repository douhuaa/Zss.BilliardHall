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
> 本项目原采用 Git Flow 分支模型，现切换为 GitHub Flow，主要原因是 GitHub Flow 更加简洁，适合持续集成和快速发布，能够减少分支管理的复杂度。对于习惯了 Git Flow 的开发人员，需注意：GitHub Flow 不再区分 develop、release、hotfix 等分支，所有新功能和修复均通过 feature 分支和 Pull Request 完成，主分支始终保持可部署状态。此变更有助于提升团队协作效率和代码质量。

- **变更原因**：GitHub Flow 更简洁，适合持续集成和频繁发布，减少分支管理复杂度。
- **影响**：开发流程更为敏捷，分支数量减少，发布流程简化。
- **迁移建议**：原有开发人员需熟悉 GitHub Flow 工作流，功能开发统一从 main 分支创建 feature 分支，合并通过 Pull Request 完成。

## 分支模型

本项目采用 **GitHub Flow** 分支模型，适合快速迭代和持续部署。

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

## 提交信息规范

### 提交信息格式

```
<类型>(<范围>): <简短描述>

<详细描述>（可选）

<关闭的Issue>（可选）
```

### 提交类型（中英文对照）

| 类型 | 英文 | 说明 | 示例 |
|------|------|------|------|
| 功能 | feat | 新功能开发 | `feat(用户): 添加用户注册功能` |
| 修复 | fix | Bug修复 | `fix(支付): 修复支付回调超时问题` |
| 文档 | docs | 文档更新 | `docs: 更新API文档和部署指南` |
| 样式 | style | 代码格式化，不影响功能 | `style: 统一代码缩进格式` |
| 重构 | refactor | 代码重构，不新增功能不修复bug | `refactor(计费): 优化计费算法逻辑` |
| 测试 | test | 添加或修改测试 | `test(用户): 增加用户注册单元测试` |
| 构建 | build | 构建系统或依赖项变更 | `build: 升级.NET版本到8.0` |
| 配置 | ci | CI/CD配置文件变更 | `ci: 添加自动化部署脚本` |
| 性能 | perf | 性能优化 | `perf(数据库): 优化查询索引策略` |
| 工具 | chore | 其他不涉及源码的变更 | `chore: 更新开发工具配置` |

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

### PR标题格式

```
[类型] 简短描述

例如：
[功能] 新增会员积分管理系统
[修复] 解决台球桌状态同步问题
[重构] 优化支付服务架构
```

### PR描述模板

```markdown
## 变更描述
简述本次PR的主要变更内容

## 变更类型
- [ ] 新功能 (feat)
- [ ] Bug修复 (fix)
- [ ] 文档更新 (docs)
- [ ] 代码重构 (refactor)
- [ ] 性能优化 (perf)
- [ ] 测试相关 (test)
- [ ] 构建相关 (build)
- [ ] 其他 (chore)

## 测试情况
- [ ] 单元测试通过
- [ ] 集成测试通过
- [ ] 手动测试验证
- [ ] 性能测试验证（如适用）

## 相关Issue
Closes #issue编号

## 额外说明
其他需要审查者注意的内容
```

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
git log --oneline --graph --decorate

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
