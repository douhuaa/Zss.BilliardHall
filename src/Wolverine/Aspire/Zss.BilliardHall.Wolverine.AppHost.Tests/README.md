# AppHost 集成测试项目

本项目包含 Aspire AppHost 的集成测试，用于验证应用编排和启动流程。

## ⚠️ 重要提示

**这些测试默认被跳过**，因为它们需要：
- Docker Desktop 正在运行
- Aspire DCP（Distributed Container Platform）支持
- 完整的容器编排环境

在 CI 环境中，这些测试会被自动跳过。

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

### `AppHost_CanStartBootstrapperService` ⚠️ 需要 Docker
验证 AppHost 能够成功启动 Bootstrapper 服务而不抛出异常。

### `Bootstrapper_HealthEndpoint_ReturnsHealthy` ⚠️ 需要 Docker
测试 Bootstrapper 的健康检查端点 `/health` 返回成功状态码。

### `Bootstrapper_RootEndpoint_ReturnsApplicationStatus` ⚠️ 需要 Docker
测试根端点 `/` 返回包含应用信息的 JSON 响应：
- 包含应用名称 `Zss.BilliardHall.Bootstrapper`
- 提及 `Wolverine` 框架
- 提及 `Marten` 框架

## 运行测试

### 前置条件
1. **Docker Desktop 已安装并运行**
   ```bash
   docker info  # 验证 Docker 可用
   ```

2. .NET 10 SDK

### 执行测试

**注意**: 这些测试默认被跳过。要运行它们，您需要：

1. **临时启用测试**：编辑 `AppHostIntegrationTests.cs`，移除 `[Fact(Skip = "...")]` 中的 `Skip` 参数

2. **手动运行**：
   ```bash
   cd src/Wolverine
   dotnet test Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests
   ```

3. **详细输出**：
   ```bash
   dotnet test Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests \
     --logger "console;verbosity=detailed"
   ```

### 跳过这些测试（默认行为）

```bash
# 运行所有测试，但排除需要 Docker 的测试
dotnet test --filter "Category!=RequiresDocker"

# 或者，在 CI 中只运行 ServiceDefaults 测试
dotnet test src/Wolverine/Aspire/Zss.BilliardHall.Wolverine.ServiceDefaults.Tests
```

## CI/CD 集成

这些测试**不会**在 CI 流程中自动运行，因为它们：
- 需要 Docker 环境（CI 环境可能不支持）
- 运行时间较长（容器启动需要时间）
- 需要完整的 Aspire DCP 基础设施

**CI 策略**:
- ✅ 运行 ServiceDefaults 单元测试（快速、无依赖）
- ❌ 跳过 AppHost 集成测试（需要 Docker）
- ✅ 验证配置完整性通过代码审查

## 本地开发验证

对于本地开发，推荐使用手动验证方式：

```bash
# 1. 启动 AppHost（会自动启动 PostgreSQL 和 Bootstrapper）
dotnet run --project src/Wolverine/Aspire/Zss.BilliardHall.Wolverine.AppHost

# 2. 访问 Aspire Dashboard
open https://localhost:17001

# 3. 验证端点
curl http://localhost:5000/        # 根端点
curl http://localhost:5000/health  # 健康检查
```

## 测试架构

使用 `Aspire.Hosting.Testing` 包提供的测试基础设施：

- `DistributedApplicationTestingBuilder` - 创建测试用 AppHost
- `CreateHttpClient()` - 获取服务的 HTTP 客户端
- 自动管理服务生命周期（启动/停止）

## 注意事项

1. **测试隔离**: 每个测试独立创建 AppHost 实例
2. **资源清理**: 使用 `await using` 确保资源正确释放
3. **超时处理**: 测试默认有 20 秒超时，容器启动可能较慢
4. **容器复用**: Docker 容器可能在测试间复用以提高速度
5. **默认跳过**: 所有测试都标记为 `Skip`，需要手动启用

## 故障排查

### 测试被跳过
这是**预期行为**。这些测试需要完整的 Docker 环境，默认不运行。

### 如果想运行这些测试

1. 确认 Docker Desktop 正在运行
2. 编辑测试文件，移除 `Skip` 参数
3. 重新运行测试

### 测试超时
- 检查 Docker 是否正常运行
- 确认 PostgreSQL 容器能够成功启动
- 第一次运行可能需要下载镜像（时间较长）

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
- [Wolverine 模块化架构蓝图](../../../../docs/03_系统架构设计/Wolverine模块化架构蓝图.md)
- [Aspire 编排架构](../../../../docs/03_系统架构设计/Aspire编排架构.md)
- [CI/CD 工作流说明](../../../../.github/workflows/README.md)

## 版本

- **创建日期**: 2026-01-11
- **.NET 版本**: 10.0
- **Aspire 版本**: 13.1.0
