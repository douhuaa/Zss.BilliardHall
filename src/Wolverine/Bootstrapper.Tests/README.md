# Bootstrapper 集成测试项目

本项目包含 Bootstrapper 应用的轻量级集成测试，**不依赖 Aspire AppHost/DCP**，使用 Testcontainers 提供隔离的测试环境。

## 📋 测试分层

本测试项目实现两层测试策略：

### 1️⃣ 烟雾测试（Smoke Tests）- `Category=Unit`
**无需 Docker**，快速验证配置完整性和服务注册。

| 测试方法 | 验证内容 |
|---------|---------|
| `BuildApp_WithValidArgs_ShouldSucceed` | 应用可以成功构建 |
| `BuildApp_ShouldRegisterHealthChecks` | HealthCheckService 正确注册 |
| `BuildApp_ShouldHaveSelfHealthCheck` | "self" 健康检查存在且标记为 "live" |
| `BuildApp_WithoutConnectionString_ShouldThrowInvalidOperationException` | 缺少连接字符串时正确抛出异常 |
| `BuildApp_ShouldRegisterMartenServices` | Marten IDocumentStore 正确注册 |
| `BuildApp_ShouldRegisterWolverineServices` | Wolverine 服务正确注册 |

**特点**：
- ✅ 使用假连接字符串，不实际连接数据库
- ✅ 快速执行（< 2秒）
- ✅ 在 CI 中每次 PR 都运行
- ✅ 验证 ServiceDefaults、Marten、Wolverine 配置正确

### 2️⃣ 集成测试（Integration Tests）- `Category=Integration+RequiresDocker`
**需要 Docker**，使用真实 PostgreSQL 容器验证完整功能。

| 测试方法 | 验证内容 |
|---------|---------|
| `Bootstrapper_WithRealDatabase_CanStartAndStop` | 应用可以正常启动和停止 |
| `Bootstrapper_HealthEndpoint_ShouldReturnHealthy` | 健康检查端点返回健康状态 |
| `Marten_CanConnectToDatabase` | 可以连接到 PostgreSQL 数据库 |
| `Marten_CanPersistAndRetrieveDocument` | 可以存储和检索文档 |
| `Marten_UsesBilliardSchema` | 使用 "billiard" schema |

**特点**：
- 🐳 使用 Testcontainers 自动管理 PostgreSQL 容器
- 🔄 测试间自动清理，完全隔离
- ⏱️ 较慢执行（首次需下载镜像，约 10-30 秒）
- 🏗️ 验证真实的数据库操作

## 🚀 运行测试

### 运行所有烟雾测试（推荐，无需 Docker）
```bash
cd src/Wolverine
dotnet test Bootstrapper.Tests \
  --filter "Category=Unit" \
  --logger "console;verbosity=normal"
```

### 运行所有集成测试（需要 Docker）
```bash
# 确保 Docker 正在运行
docker info

cd src/Wolverine
dotnet test Bootstrapper.Tests \
  --filter "Category=Integration" \
  --logger "console;verbosity=normal"
```

### 运行所有测试
```bash
cd src/Wolverine
dotnet test Bootstrapper.Tests \
  --logger "console;verbosity=normal"
```

### 运行特定测试
```bash
cd src/Wolverine
dotnet test Bootstrapper.Tests \
  --filter "FullyQualifiedName~BuildApp_WithValidArgs_ShouldSucceed"
```

## 🏗️ 架构设计

### BootstrapperHost 抽象
测试通过 `BootstrapperHost` 静态类构建应用，避免直接依赖 `Program.cs`：

```csharp
// 使用默认参数
var app = BootstrapperHost.BuildApp(args);

// 使用自定义 builder（测试场景）
var builder = WebApplication.CreateBuilder();
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ConnectionStrings:Default"] = testConnectionString
});
var app = BootstrapperHost.BuildAppWithBuilder(builder);
```

### PostgresFixture 生命周期
使用 xUnit 的 `IClassFixture<PostgresFixture>` 管理测试容器：

```csharp
public class BootstrapperIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public BootstrapperIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SomeTest()
    {
        // 使用 _fixture.ConnectionString 连接测试数据库
    }
}
```

**容器生命周期**：
- `InitializeAsync()` - 测试类开始前启动容器
- 所有测试共享同一个容器实例
- `DisposeAsync()` - 测试类完成后自动清理容器

## 📦 依赖项

| 包名 | 版本 | 用途 |
|-----|------|-----|
| `Testcontainers` | 4.4.0 | 容器编排基础设施 |
| `Testcontainers.PostgreSql` | 4.4.0 | PostgreSQL 容器支持 |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.0 | ASP.NET Core 测试工具 |
| `FluentAssertions` | 8.8.0 | 流畅断言 |
| `xunit` | 2.9.3 | 测试框架 |

## 🔄 与 AppHost.Tests 的区别

| 特性 | Bootstrapper.Tests | AppHost.Tests |
|-----|-------------------|---------------|
| **依赖** | Testcontainers（仅 Docker） | Aspire DCP + Docker |
| **启动速度** | 快（< 5秒，烟雾测试 < 2秒） | 慢（需启动完整编排） |
| **CI 运行** | ✅ 每次 PR | ❌ 仅手动触发/push main |
| **测试范围** | Bootstrapper 配置 + Marten + Wolverine | 完整应用编排 + 多服务协同 |
| **失败原因** | 配置问题、代码错误 | 环境问题、超时、编排失败 |
| **用途** | 快速反馈、本地开发 | E2E 验证、生产环境模拟 |

## 🧪 CI/CD 集成

### GitHub Actions 配置
```yaml
- name: Run Bootstrapper smoke tests
  run: |
    dotnet test \
      src/Wolverine/Bootstrapper.Tests/Zss.BilliardHall.Wolverine.Bootstrapper.Tests.csproj \
      --filter "Category=Unit" \
      -c Release \
      --no-build \
      --logger "trx;LogFileName=bootstrapper-smoke-test-results.trx"
```

**CI 策略**：
- ✅ PR 检查：烟雾测试（无 Docker）
- ⏸️ 集成测试：本地开发手动运行
- 🔄 Nightly：可选择运行完整集成测试

## 📝 测试最佳实践

### 1. 遵循 AAA 模式
```csharp
[Fact]
public async Task Marten_CanPersistAndRetrieveDocument()
{
    // Arrange - 准备测试数据
    var testDoc = new TestDocument { Id = Guid.NewGuid(), Name = "Test" };

    // Act - 执行操作
    using (var session = documentStore.LightweightSession())
    {
        session.Store(testDoc);
        await session.SaveChangesAsync();
    }

    // Assert - 验证结果
    retrieved.Should().NotBeNull();
}
```

### 2. 使用 FluentAssertions
```csharp
result.Status.Should().Be(HealthStatus.Healthy, 
    "Health check should pass");
```

### 3. 测试命名规范
遵循 `MethodName_Condition_ExpectedResult` 模式：
- `BuildApp_WithValidArgs_ShouldSucceed`
- `Marten_CanPersistAndRetrieveDocument`

### 4. 分类标记
使用 `[Trait]` 标记测试类别：
```csharp
[Trait("Category", "Unit")]         // 烟雾测试
[Trait("Category", "Integration")]  // 集成测试
[Trait("Category", "RequiresDocker")] // 需要 Docker
```

## 🐛 故障排查

### 烟雾测试失败
**原因**：配置问题、服务注册错误、代码编译错误

**解决**：
```bash
# 检查构建
dotnet build src/Wolverine/Bootstrapper/Bootstrapper.csproj

# 查看详细测试输出
dotnet test Bootstrapper.Tests --filter "Category=Unit" -v detailed
```

### 集成测试失败（Docker 相关）
**症状**：容器启动失败、连接超时

**解决**：
```bash
# 1. 确认 Docker 运行
docker info

# 2. 检查镜像
docker images | grep postgres

# 3. 手动拉取镜像（首次可能较慢）
docker pull postgres:latest

# 4. 清理悬空容器
docker container prune -f
```

### 集成测试失败（Marten 相关）
**症状**：Schema 错误、连接字符串无效

**解决**：
```bash
# 检查 PostgresFixture 日志
dotnet test Bootstrapper.Tests --filter "Category=Integration" -v detailed

# 确认测试容器已启动
docker ps
```

## 📚 相关文档

- [Wolverine 模块化架构蓝图](../../../docs/03_系统架构设计/Wolverine模块化架构蓝图.md)
- [Wolverine 快速上手指南](../../../docs/03_系统架构设计/Wolverine快速上手指南.md)
- [ServiceDefaults 集成指南](../../../docs/06_开发规范/ServiceDefaults集成指南.md)
- [测试入口说明](../测试入口说明.md)
- [AppHost.Tests README](../Aspire/Zss.BilliardHall.Wolverine.AppHost.Tests/README.md)

## 📊 测试统计

- **总测试数**：11
  - 烟雾测试（Unit）：6
  - 集成测试（Integration）：5
- **平均执行时间**：
  - 烟雾测试：< 2 秒
  - 集成测试：5-10 秒（首次启动容器更长）

## 🎯 未来改进

- [ ] 添加 Wolverine Handler 集成测试
- [ ] 添加消息总线（IMessageBus）测试
- [ ] 添加事件发布/订阅测试
- [ ] 添加性能基准测试

---

**创建日期**: 2026-01-11  
**.NET 版本**: 10.0  
**测试框架**: xUnit 2.9.3  
**Testcontainers 版本**: 4.4.0
