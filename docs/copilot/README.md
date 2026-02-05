# Copilot 提示词库

> ⚠️ 本文档不具备裁决力。所有架构决策以对应 ADR 正文为准。

## 目录说明

GitHub Copilot 场景化提示词。

## Constitutional（宪法层）

- [adr-900.prompts.md](adr-900.prompts.md) - 架构测试与 CI 治理
- [adr-001.prompts.md](adr-001.prompts.md) - 模块化单体与垂直切片架构
- [adr-002.prompts.md](adr-002.prompts.md) - Platform/Application/Host 三层启动体系
- [adr-003.prompts.md](adr-003.prompts.md) - 命名空间与项目边界规范
- [adr-004.prompts.md](adr-004.prompts.md) - 中央包管理（CPM）规范
- [adr-005.prompts.md](adr-005.prompts.md) - 应用内交互模型与执行边界
- [adr-006.prompts.md](adr-006.prompts.md) - 术语与编号宪法
- [adr-007.prompts.md](adr-007.prompts.md) - Agent 行为与权限宪法
- [adr-008.prompts.md](adr-008.prompts.md) - 文档编写与维护宪法

## Structure（结构层）

- [adr-120.prompts.md](adr-120.prompts.md) - 领域事件命名规范
- [adr-121.prompts.md](adr-121.prompts.md) - 契约（Contract）与 DTO 命名组织规范
- [adr-122.prompts.md](adr-122.prompts.md) - Repository 命名及组织规范
- [adr-123.prompts.md](adr-123.prompts.md) - 服务接口命名规范
- [adr-124.prompts.md](adr-124.prompts.md) - Endpoint 命名及参数约束规范

## Runtime（运行层）

- [adr-201.prompts.md](adr-201.prompts.md) - Handler 生命周期管理
- [adr-210.prompts.md](adr-210.prompts.md) - 领域事件版本化与兼容性
- [adr-220.prompts.md](adr-220.prompts.md) - 事件总线集成规范
- [adr-240.prompts.md](adr-240.prompts.md) - Handler 异常约束

## Technical（技术层）

- [adr-301.prompts.md](adr-301.prompts.md) - Marten 作为事件存储与文档数据库
- [adr-340.prompts.md](adr-340.prompts.md) - MassTransit 作为事件总线
- [adr-350.prompts.md](adr-350.prompts.md) - ASP.NET Core Minimal API
- [adr-360.prompts.md](adr-360.prompts.md) - Serilog 结构化日志

## Governance（治理层）

- [adr-0900.prompts.md](adr-0900.prompts.md) - ADR 流程
- [adr-905.prompts.md](adr-905.prompts.md) - 执行级别分类
- [adr-910.prompts.md](adr-910.prompts.md) - README 编写与维护治理规范
- [adr-920.prompts.md](adr-920.prompts.md) - 示例代码治理规范
- [adr-930.prompts.md](adr-930.prompts.md) - 代码评审合规性
- [adr-940.prompts.md](adr-940.prompts.md) - ADR 关系与溯源管理治理规范
- [adr-945.prompts.md](adr-945.prompts.md) - ADR 时间线演进视图
- [adr-946.prompts.md](adr-946.prompts.md) - ADR 标题级别即语义级别约束
- [adr-950.prompts.md](adr-950.prompts.md) - 指南与 FAQ 文档治理
- [adr-951.prompts.md](adr-951.prompts.md) - 案例库管理
- [adr-952.prompts.md](adr-952.prompts.md) - 工程标准与 ADR 边界
- [adr-955.prompts.md](adr-955.prompts.md) - 文档搜索与可发现性
- [adr-960.prompts.md](adr-960.prompts.md) - Onboarding 文档治理
- [adr-965.prompts.md](adr-965.prompts.md) - Onboarding 交互式学习路径
- [adr-970.prompts.md](adr-970.prompts.md) - 自动化日志集成标准
- [adr-975.prompts.md](adr-975.prompts.md) - 文档质量监控
- [adr-980.prompts.md](adr-980.prompts.md) - ADR 生命周期同步
- [adr-990.prompts.md](adr-990.prompts.md) - 文档演进路线图

## 其他

- [architecture-test-failures.md](architecture-test-failures.md) - 架构测试失败诊断指南
- [governance-compliance.prompts.md](governance-compliance.prompts.md) - 治理合规性提示
- [pr-common-issues.prompts.md](pr-common-issues.prompts.md) - PR 常见问题总结
- [pr-review-pipeline.md](pr-review-pipeline.md) - PR 评审流程

## 链接

- [返回上级](../README.md) | [ADR 目录](../adr/README.md) | [Instructions](../../.github/instructions/)
