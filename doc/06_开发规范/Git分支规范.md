# Git 分支规范

## 分支模型

本项目采用 **Git Flow** 分支模型，适合团队协作开发和版本发布管理。

### 主要分支

#### 1. main / master（主分支）
- **用途**：生产环境代码，始终保持稳定可部署状态
- **保护规则**：
  - 禁止直接 push
  - 必须通过 Pull Request 合并
  - 需要至少 1 人代码审查
- **合并来源**：仅接受来自 release 和 hotfix 分支的合并

#### 2. develop（开发分支）
- **用途**：开发环境主分支，集成所有功能开发
- **合并来源**：feature 分支合并目标
- **合并目标**：release 分支的创建来源

#### 3. feature/*（功能分支）
- **命名规范**：
  ```
  feature/用户管理模块
  feature/支付系统集成
  feature/台球桌预约功能
  ```
- **生命周期**：从 develop 创建，完成后合并回 develop
- **删除规则**：合并后删除分支

#### 4. release/*（发布分支）
- **命名规范**：
  ```
  release/v1.0.0
  release/v1.1.0-台球桌管理优化
  ```
- **用途**：准备生产发布，进行最终测试和 bug 修复
- **生命周期**：从 develop 创建，同时合并到 main 和 develop

#### 5. hotfix/*（热修复分支）
- **命名规范**：
  ```
  hotfix/支付回调异常修复
  hotfix/v1.0.1-紧急安全补丁
  ```
- **生命周期**：从 main 创建，同时合并到 main 和 develop

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

### 1. 功能开发流程

```bash
# 1. 从develop创建功能分支
git checkout develop
git pull origin develop
git checkout -b feature/会员积分系统

# 2. 开发过程中定期提交
git add .
git commit -m "feat(积分): 实现积分计算核心逻辑"

# 3. 推送到远程仓库
git push -u origin feature/会员积分系统

# 4. 创建Pull Request到develop分支
# 5. 代码审查通过后合并
# 6. 删除功能分支
git branch -d feature/会员积分系统
git push origin --delete feature/会员积分系统
```

### 2. 发布流程

```bash
# 1. 从develop创建发布分支
git checkout develop
git pull origin develop
git checkout -b release/v1.2.0

# 2. 进行发布前的最终调整
git commit -m "chore(版本): 更新版本号到v1.2.0"

# 3. 合并到main分支
git checkout main
git pull origin main
git merge --no-ff release/v1.2.0
git tag -a v1.2.0 -m "发布版本v1.2.0：会员积分系统上线"

# 4. 合并回develop分支
git checkout develop
git merge --no-ff release/v1.2.0

# 5. 推送所有变更
git push origin main
git push origin develop
git push origin v1.2.0

# 6. 删除发布分支
git branch -d release/v1.2.0
git push origin --delete release/v1.2.0
```

### 3. 热修复流程

```bash
# 1. 从main创建热修复分支
git checkout main
git pull origin main
git checkout -b hotfix/支付异常修复

# 2. 修复问题
git commit -m "fix(支付): 修复支付状态更新异常

问题：支付成功后状态未正确更新导致重复扣费
解决：添加事务保证状态更新原子性

Fixes #789"

# 3. 合并到main和develop
git checkout main
git merge --no-ff hotfix/支付异常修复
git tag -a v1.2.1 -m "热修复版本v1.2.1：支付异常修复"

git checkout develop
git merge --no-ff hotfix/支付异常修复

# 4. 推送变更
git push origin main
git push origin develop
git push origin v1.2.1
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

## 分支保护规则

### main分支保护
- 禁止直接push
- 必须通过PR合并
- 需要至少1人代码审查
- 必须通过CI/CD检查
- 必须是最新代码（不允许过时的分支合并）

### develop分支保护
- 允许管理员直接push
- PR需要代码审查
- 必须通过自动化测试

## 常用Git命令

```bash
# 查看分支情况
git branch -a
git branch -r

# 清理本地分支
git branch --merged | grep -v "\*\|main\|develop" | xargs -n 1 git branch -d

# 更新本地分支列表
git fetch --prune origin

# 查看提交历史
git log --oneline --graph --decorate

# 查看某个文件的变更历史
git log --follow --patch -- 文件名
```

---

📝 **相关文档**：
- [代码风格指南](代码风格.md)
- [Code Review流程](CodeReview流程.md)
- [发布管理流程](../12_版本与变更管理/发布管理流程.md)
