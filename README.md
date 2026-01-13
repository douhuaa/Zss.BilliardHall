# 自助台球系统 (Zss.BilliardHall)

> 基于 Wolverine + 垂直切片架构的现代化自助台球管理系统

---

## 项目简介

自助台球系统是一个采用 **Wolverine + Marten + 垂直切片架构** 构建的现代化台球馆管理系统。项目摒弃传统分层架构，采用垂直切片（Vertical Slice）方式组织代码，实现高内聚、低耦合的模块化设计。

### 核心特性

- ✅ **垂直切片架构**: 每个功能独立组织，代码高度内聚
- ✅ **Wolverine 框架**: 强大的消息处理和工作流引擎
- ✅ **Marten 持久化**: 基于 PostgreSQL 的文档数据库
- ✅ **事件驱动**: 模块间通过消息/事件通信
- ✅ **自动事务**: `[Transactional]` 特性自动管理事务
- ✅ **Outbox 模式**: 保证消息可靠投递

---

## 快速开始

### 前置要求

- .NET 8.0+
- PostgreSQL 14+
- Docker（可选，用于本地开发）

### 克隆项目

```bash
git clone https://github.com/douhuaa/Zss.BilliardHall.git
cd Zss.BilliardHall
```

### 运行项目

```bash
# 启动依赖服务（PostgreSQL）
docker-compose up -d

# 运行应用
dotnet run --project src/Wolverine/Wolverine.csproj
```

### 访问应用

- API 地址: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

---

## 项目结构

```
├── .github/                    # GitHub 配置
│   ├── copilot-templates.md    # Copilot 模板（重要！）
│   ├── copilot-quick-start.md  # 快速开始指南
│   └── copilot-instructions.md # 架构规范
├── docs/                       # 完整文档
│   ├── 03_系统架构设计/        # 架构设计文档
│   ├── 04_模块设计/            # 模块详细设计
│   └── 06_开发规范/            # 开发规范和指南
└── src/
    ├── Wolverine/              # 主应用（Wolverine）
    │   └── Modules/            # 业务模块
    │       ├── Members/        # 会员管理
    │       ├── Sessions/       # 打球时段
    │       └── Tables/         # 台球桌管理
    └── Zss.BilliardHall/       # 遗留代码（待迁移）
```

---

## 🚀 开发指南

### 新开发者必读

1. **阅读架构文档** (15分钟)
   - [Wolverine 快速上手指南](./docs/03_系统架构设计/Wolverine快速上手指南.md)
   - [Wolverine 模块化架构蓝图](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md)

2. **学习 Copilot 模板** (5分钟)
   - [Copilot 快速开始](./.github/copilot-quick-start.md) ⭐ **强烈推荐**
   - [Copilot 完整模板](./.github/copilot-templates.md)

3. **参考现有代码**
   - 查看 `src/Wolverine/Modules/Members/RegisterMember/` 完整示例

### 创建新功能（使用 Copilot）

以创建"取消订单"功能为例：

1. 创建功能文件夹 `Modules/Orders/CancelOrder/`
2. 在 `CancelOrder.cs` 中输入 Copilot 指令：

```csharp
// 创建一个完整的垂直切片功能：取消订单
// 模块：Orders
// 功能：CancelOrder
// Command 参数：Guid orderId, string reason
// 业务逻辑：
//   1. 加载订单
//   2. 验证订单状态可取消
//   3. 取消订单
//   4. 返回 Result<Unit> 和 OrderCancelled 事件
// Handler 使用 [Transactional]
// Endpoint 使用 WolverinePut，路径 /api/orders/{orderId}/cancel
// 添加 FluentValidation 验证器
```

3. Copilot 自动生成完整代码
4. 检查并调整业务逻辑

**详见**: [Copilot 快速开始指南](./.github/copilot-quick-start.md)

---

## 核心模块

### Members（会员管理）

- 注册会员
- 充值余额
- 扣除余额
- 积分奖励
- 会员等级管理

### Sessions（打球时段）

- 开始会话
- 暂停/恢复会话
- 结束会话
- 会话生命周期 Saga

### Tables（台球桌管理）

- 创建台球桌
- 预订/释放
- 状态管理
- 维护管理

---

## 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0+ | 应用框架 |
| Wolverine | 3.x | 消息处理和工作流 |
| Marten | 7.x | 文档数据库（基于 PostgreSQL） |
| FluentValidation | 11.x | 输入验证 |
| Serilog | 3.x | 结构化日志 |
| xUnit | 2.x | 单元测试 |

---

## 开发规范

### 架构原则

- ✅ **100% 垂直切片**: 拒绝传统分层架构
- ✅ **Handler 即 Application Service**: 不创建额外的 Service 层
- ✅ **事件驱动通信**: 模块间通过消息/事件通信
- ❌ **禁止共享 Service**: 避免创建跨模块的共享服务
- ❌ **禁止 Repository 接口**: 直接使用 `IDocumentSession`

### 代码风格

- 命名：PascalCase（类/接口/公共成员）、camelCase + `_`（私有字段）
- 使用 `record` 定义 Command/Query/Event
- Handler 使用 `[Transactional]` 特性
- 优先使用 UTC 时间（`DateTimeOffset.UtcNow`）
- 异步方法接受 `CancellationToken cancellationToken = default`

**完整规范**: [代码风格规范](./docs/06_开发规范/代码风格.md)

---

## 测试

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定模块测试
dotnet test --filter "FullyQualifiedName~Members"
```

### 测试策略

- **单元测试**: 测试 Handler 业务逻辑（使用 In-Memory Marten）
- **集成测试**: 测试完整流程（使用真实 Wolverine 配置）
- **契约测试**: 测试跨模块通信（Event 契约）

---

## 文档

### 📚 文档结构

| 章节 | 主要内容 | 快速跳转 |
|------|----------|----------|
| **🎯 项目概述** | 背景目标、使用场景 | [01_项目概述](./docs/01_项目概述/) |
| **📋 需求规格** | 功能需求、用例图 | [02_需求规格说明](./docs/02_需求规格说明/) |
| **🏗️ 系统架构** | 技术选型、架构图 | [03_系统架构设计](./docs/03_系统架构设计/) ⭐ |
| **⚙️ 模块设计** | 模块划分、接口定义 | [04_模块设计](./docs/04_模块设计/) |
| **🗃️ 数据库设计** | Marten/EF Core、表结构 | [05_数据库设计](./docs/05_数据库设计/) |
| **📝 开发规范** | Git规范、Code Review | [06_开发规范](./docs/06_开发规范/) |
| **🔧 API文档** | 接口规范、认证授权 | [07_API文档](./docs/07_API文档/) |
| **⚙️ 配置管理** | 环境配置、密钥管理 | [08_配置管理](./docs/08_配置管理/) |
| **🧪 测试方案** | 测试策略、用例设计 | [09_测试方案](./docs/09_测试方案/) |
| **🚀 部署运维** | 部署流程、监控方案 | [10_部署与运维](./docs/10_部署与运维/) |
| **📖 用户手册** | 操作指南、FAQ | [11_用户手册](./docs/11_用户手册/) |
| **🔄 版本管理** | 变更日志、发布流程 | [12_版本与变更管理](./docs/12_版本与变更管理/) |
| **🎓 培训交付** | 培训材料、知识转移 | [13_培训与交付](./docs/13_培训与交付/) |

### 🎓 学习路径

#### 新成员入职（推荐5天）

**Day 1: 架构理念**
- [Copilot 快速开始](./.github/copilot-quick-start.md) (5分钟) - 学会使用 AI 辅助开发
- [架构变更摘要](./docs/03_系统架构设计/架构变更摘要.md) (30分钟) - 了解架构演进
- [垂直切片架构说明](./docs/03_系统架构设计/垂直切片架构说明.md) (1小时) - 理解核心架构

**Day 2-3: 动手实践**
- [Wolverine 快速上手指南](./docs/03_系统架构设计/Wolverine快速上手指南.md) ⭐ (2小时) - 实现第一个功能
- [Wolverine 模块化架构蓝图](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md) ⭐⭐⭐ (3-4小时) - 深入理解架构

**Day 4: 模块边界**
- [系统模块划分](./docs/03_系统架构设计/系统模块划分.md) (1小时) - 了解模块边界
- [Aspire编排架构](./docs/03_系统架构设计/Aspire编排架构.md) (1小时) - 服务编排

**Day 5: 规范与实践**
- [开发规范](./docs/06_开发规范/) - 代码风格、Saga 使用等
- 在导师指导下实现第一个功能切片

#### 架构师/Tech Lead

1. 完整阅读 [Wolverine模块化架构蓝图](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md)
2. 学习 [设计原则](./docs/03_系统架构设计/设计原则.md) 和 [系统模块划分](./docs/03_系统架构设计/系统模块划分.md)
3. 参考 [技术选型](./docs/03_系统架构设计/技术选型.md) 了解技术决策
4. 阅读 [Wolverine](https://wolverine.netlify.app/) 和 [Marten](https://martendb.io/) 官方文档

### ❓ 常见问题速查

**Q: 如何实现一个新功能？**
- 5步搞定：创建目录 → 定义Command → 实现Handler → 添加Endpoint → 运行测试
- 详见: [Wolverine快速上手指南 - 第一章](./docs/03_系统架构设计/Wolverine快速上手指南.md#一5-分钟上手)

**Q: Handler 如何自动注入依赖？**
- Wolverine 约定：第一个参数是消息，后续参数自动注入（`IDocumentSession`、`ILogger` 等）
- 详见: [Wolverine快速上手指南 - 2.1节](./docs/03_系统架构设计/Wolverine快速上手指南.md#21-wolverine-的-3-个核心约定)

**Q: 模块间如何通信？**
- 同步: `await bus.InvokeAsync(command)`（仅进程内）
- 异步: `await bus.PublishAsync(event)`
- 详见: [Wolverine模块化架构蓝图 - 第六章](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md#六跨模块通信底线规则)

**Q: 如何实现事务？**
- 使用 `[Transactional]` 特性或全局策略
- 详见: [Wolverine快速上手指南 - 3.5节](./docs/03_系统架构设计/Wolverine快速上手指南.md#35-场景-5添加事务支持)

### 核心文档快速索引

| 文档 | 说明 | 适合人群 |
|------|------|----------|
| [Copilot 快速开始](./.github/copilot-quick-start.md) | 5分钟上手 Copilot 模板 | 🔰 新人必读 |
| [Copilot 完整模板](./.github/copilot-templates.md) | 详细的代码生成模板 | 所有开发者 |
| [Wolverine 快速上手](./docs/03_系统架构设计/Wolverine快速上手指南.md) | 15分钟上手 Wolverine | 🔰 新人必读 |
| [Wolverine 架构蓝图](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md) | 完整架构设计文档 | 架构师/核心开发 |
| [开发规范](./docs/06_开发规范/) | 代码风格、日志、Saga 等规范 | 所有开发者 |

---

## 贡献指南

### 提交代码

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'feat(module): 添加某功能'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

### Commit 规范

使用 [Conventional Commits](https://www.conventionalcommits.org/) 格式：

```
feat(module): 添加新功能
fix(module): 修复 bug
docs: 更新文档
refactor(module): 重构代码
test(module): 添加测试
chore: 其他修改
```

**示例**:
- `feat(members): 支持会员充值功能`
- `fix(sessions): 修正会话暂停后无法恢复的问题`
- `docs(copilot): 更新 Copilot 模板文档`

---

## 常见问题

### Q1: 为什么不使用传统的分层架构？

**A**: 传统分层（Application/Domain/Infrastructure）在 Wolverine 架构中会稀释框架优势。垂直切片架构让每个功能独立、内聚，更易维护和演进。

**详见**: [架构蓝图 - 核心原则](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md#一总体架构立场)

### Q2: 如何跨模块通信？

**A**: 
- 同步调用: `IMessageBus.InvokeAsync<TResult>(command)`（仅限进程内）
- 异步事件: `IMessageBus.PublishAsync(event)`
- 禁止: 创建共享 Service 或直接调用其他模块 Handler

### Q3: 如何使用 Copilot 提高开发效率？

**A**: 
1. 阅读 [Copilot 快速开始指南](./.github/copilot-quick-start.md)（5分钟）
2. 使用模板快速生成代码框架
3. 手动补充核心业务逻辑

**效率提升**: 使用 Copilot 模板后，开发效率可提升 3-5 倍

---

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

---

## 联系方式

- 项目维护: [@douhuaa](https://github.com/douhuaa)
- Issue: https://github.com/douhuaa/Zss.BilliardHall/issues

---

**⭐ 如果觉得项目不错，请点个 Star 支持一下！**
