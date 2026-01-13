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

### 核心文档

| 文档 | 说明 | 适合人群 |
|------|------|----------|
| [Copilot 快速开始](./.github/copilot-quick-start.md) | 5分钟上手 Copilot 模板 | 🔰 新人必读 |
| [Copilot 完整模板](./.github/copilot-templates.md) | 详细的代码生成模板 | 所有开发者 |
| [Wolverine 快速上手](./docs/03_系统架构设计/Wolverine快速上手指南.md) | 15分钟上手 Wolverine | 🔰 新人必读 |
| [Wolverine 架构蓝图](./docs/03_系统架构设计/Wolverine模块化架构蓝图.md) | 完整架构设计文档 | 架构师/核心开发 |
| [开发规范](./docs/06_开发规范/) | 代码风格、日志、Saga 等规范 | 所有开发者 |

### 文档导航

```
开发者旅程：
1. [Copilot 快速开始] → 5分钟学会生成代码
2. [Wolverine 快速上手] → 15分钟理解框架
3. [架构蓝图] → 深入理解架构设计
4. [开发规范] → 掌握代码规范
```

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
