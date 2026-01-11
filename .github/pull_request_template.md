<!--
统一 PR 模板
说明：保留对当前变更真正有价值的勾选；无关项可删除，保持 PR 聚焦。
-->

# Pull Request 模板

## 变更概述

> 一句话说明 + 必要背景 / 动机。

## 变更类型

- [ ] feat 新功能
- [ ] fix 缺陷修复
- [ ] docs 文档/注释
- [ ] refactor 重构（无行为变化）
- [ ] perf 性能优化
- [ ] test 测试相关
- [ ] build 构建/脚本
- [ ] ci CI/CD 配置
- [ ] deps 依赖升级/替换
- [ ] security 安全修复/加固
- [ ] revert 回滚提交
- [ ] chore 杂项维护

## 主要改动 / 结构

- 模块：
- 关键文件：
- 设计要点：

## 评审关注点 (务必精简 2~5 项)

1.
2.
3.

## 风险与回滚

- 潜在风险：
- 回滚策略： (Git revert / Feature Flag / DB 脚本)

## 兼容性影响

- [ ] 无破坏性改动
- [ ] 存在潜在不兼容（已在提交体或描述中标注 BREAKING CHANGE）

说明：

## 测试验证

- [ ] 单元测试新增/更新
- [ ] 单元测试全部通过
- [ ] 集成/接口测试通过
- [ ] 覆盖率未下降（或已说明原因）
- [ ] 手动关键路径验证
- [ ] 性能冒烟（如适用）

## 数据库 / 迁移

- [ ] 无 DB 结构变更
- [ ] 包含迁移 (命名符合规范)
- [ ] 已在本地/测试库验证迁移

## 安全与日志

- [ ] 未新增硬编码 Secret
- [ ] 未记录敏感信息到日志
- [ ] 关键失败路径有结构化日志

## 架构 / Wolverine 垂直切片

- [ ] 垂直切片结构正确（UseCase = 文件夹，包含 Command/Handler/Endpoint）
- [ ] Handler 使用 `[Transactional]` 自动事务（Marten/EF Core）
- [ ] 跨模块通信通过事件（`PublishAsync`）或内部调用（`InvokeAsync`），无 Shared Service
- [ ] Endpoint 只做映射，业务逻辑在 Handler
- [ ] 直接使用 `IDocumentSession`（Marten）或 `DbContext`（EF Core），无 Repository 接口
- [ ] 无 Application/Domain/Infrastructure 分层结构

## Issue 关联

Closes #
Refs #

## 截图 / 录屏（可选）

> UI 或交互变更时附图/短视频。

## 其他说明

> 额外上下文 / 后续计划 / 延期处理事项。

---
Reviewer Checklist（勾选由 Reviewer 或作者在评论中补充）

- [ ] 代码可读性良好
- [ ] 没有明显重复 / 可抽取逻辑
- [ ] 异常与边界条件覆盖
- [ ] 日志与监控字段充分
- [ ] 无安全/隐私风险
- [ ] 提交记录整洁（必要时 squash）
