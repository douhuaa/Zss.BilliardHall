# 文档目录

## 架构文档

### 核心文档
- **[架构指南](architecture-guide.md)** - 项目架构概述、设计规则和开发指南
- **[CI/CD 指南](ci-cd-guide.md)** - 持续集成、架构测试集成和违规处理

### 架构决策记录 (ADR)
- [ADR-0001: 模块化单体与垂直切片架构决策](adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)

## 模块文档

### 业务模块
- [Members 模块](../src/Modules/Members/README.md) - 会员管理
- [Orders 模块](../src/Modules/Orders/README.md) - 订单管理

### 平台与基础设施
- [Platform.Contracts](../src/Platform/Contracts/README.md) - 数据契约使用指南

### 测试
- [Architecture Tests](../src/tests/ArchitectureTests/README.md) - 架构测试说明

## 快速开始

### 理解架构
1. 阅读 [架构指南](architecture-guide.md) 了解整体架构
2. 阅读 [ADR-0001](adr/ADR-0001-modular-monolith-vertical-slice-architecture.md) 了解架构决策背景

### 开发新功能
1. 参考 [架构指南 - 开发指南](architecture-guide.md#开发指南)
2. 查看模块示例代码（Members/Orders）
3. 运行架构测试验证

### 处理 CI 失败
1. 参考 [CI/CD 指南 - 处理架构违规](ci-cd-guide.md#处理架构违规)
2. 理解失败原因并修复

## 架构原则

### 核心原则
1. **模块隔离** - 业务模块之间不能直接相互引用
2. **垂直切片** - 按用例组织功能，而非技术层
3. **自动化测试** - 所有架构约束通过测试强制执行

### 不可协商条款
1. 禁止模块直接引用其他模块实现
2. 所有隔离规则须自动化测试校验
3. 架构测试失败即构建失败
4. 契约不得驱动业务决策
5. ADR 为最终裁决依据

## 贡献指南

### 添加新模块
1. 在 `src/Modules/` 下创建模块目录
2. 创建模块 README 说明职责
3. 按垂直切片组织功能
4. 运行架构测试验证

### 更新架构规则
1. 创建新的 ADR 文档
2. 更新相关架构测试
3. 更新架构指南
4. 通知团队成员

### 文档维护
- 文档应保持最新
- 代码变更同步更新文档
- 定期评审文档准确性

## 资源链接

### 外部参考
- [Vertical Slice Architecture - Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Modular Monolith - Kamil Grzybek](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer)
- [NetArchTest.Rules](https://github.com/BenMorris/NetArchTest)

### 工具
- [NetArchTest](https://github.com/BenMorris/NetArchTest) - 架构测试框架
- [Wolverine](https://wolverinefx.net/) - 消息总线和中介者模式
- [Marten](https://martendb.io/) - 事件存储和文档数据库
