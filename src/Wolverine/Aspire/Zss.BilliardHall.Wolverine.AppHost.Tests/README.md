# AppHost 集成测试项目

本项目包含 Aspire AppHost 的集成测试，用于验证应用编排和启动流程。

## 测试目的

验证"阶段一：基础设施就绪"的核心目标：

1. **配置完整性**
   - Serilog 配置从 appsettings.json 正确加载
   - ServiceDefaults / Wolverine / Marten 正常配置
   - AppHost 能够启动 PostgreSQL 和 Bootstrapper

2. **应用可运行性**
   - AppHost 能够成功启动
   - Bootstrapper 服务不抛异常退出
   - 健康检查通过
   - 基本路由可访问

## 测试列表

### `AppHost_CanStartBootstrapperService`
验证 AppHost 能够成功启动 Bootstrapper 服务而不抛出异常。

### `Bootstrapper_HealthEndpoint_ReturnsHealthy`
测试 Bootstrapper 的健康检查端点 `/health` 返回成功状态码。

### `Bootstrapper_RootEndpoint_ReturnsApplicationStatus`
测试根端点 `/` 返回包含应用信息的 JSON 响应：
- 包含应用名称 `Zss.BilliardHall.Bootstrapper`
- 提及 `Wolverine` 框架
- 提及 `Marten` 框架

## 运行测试

### 前置条件
- Docker 已安装并运行（用于 PostgreSQL 容器）
- .NET 10 SDK

### 执行测试

```bash
# 运行所有 AppHost 测试
cd src/Wolverine
dotnet test Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests

# 运行特定测试
dotnet test Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests --filter FullyQualifiedName~Bootstrapper_HealthEndpoint

# 详细输出
dotnet test Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests --logger "console;verbosity=detailed"
```

## CI/CD 集成

这些测试适合在 CI 流程中自动运行，作为 PR 验证的一部分。

**建议的 CI 步骤**:
1. 启动 Docker（如果未运行）
2. 运行 AppHost 集成测试
3. 验证所有测试通过
4. 清理容器资源

## 测试架构

使用 `Aspire.Hosting.Testing` 包提供的测试基础设施：

- `DistributedApplicationTestingBuilder` - 创建测试用 AppHost
- `CreateHttpClient()` - 获取服务的 HTTP 客户端
- 自动管理服务生命周期（启动/停止）

## 注意事项

1. **测试隔离**: 每个测试独立创建 AppHost 实例
2. **资源清理**: 使用 `await using` 确保资源正确释放
3. **超时处理**: 长时间运行的测试可能需要调整超时设置
4. **容器复用**: Docker 容器可能在测试间复用以提高速度

## 故障排查

### 测试超时
- 检查 Docker 是否正常运行
- 确认 PostgreSQL 容器能够成功启动
- 增加测试超时时间

### 健康检查失败
- 查看 Bootstrapper 启动日志
- 验证数据库连接字符串
- 检查端口占用

### 连接被拒绝
- 确认服务已完全启动
- 检查防火墙设置
- 验证服务发现配置

## 相关文档

- [Aspire Testing Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Wolverine 模块化架构蓝图](../../../../doc/03_系统架构设计/Wolverine模块化架构蓝图.md)
- [Aspire 编排架构](../../../../doc/03_系统架构设计/Aspire编排架构.md)

## 版本

- **创建日期**: 2026-01-11
- **.NET 版本**: 10.0
- **Aspire 版本**: 13.1.0
