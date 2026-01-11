# Zss.BilliardHall.Wolverine.ServiceDefaults

## 概述

ServiceDefaults 是台球厅管理系统所有服务共享的基础设施配置库，基于 .NET Aspire 标准模板构建。

**核心功能**:
- ✅ 服务发现（Service Discovery）
- ✅ 健康检查（Health Checks）
- ✅ OpenTelemetry 可观测性（日志、指标、追踪）
- ✅ HTTP 客户端弹性（重试、断路器、超时）
- ✅ Wolverine 命令总线与消息总线（Command Bus & Message Bus）
- ✅ 标准化配置

---

## 快速开始

### 1. 添加项目引用

在服务项目（如 `Bootstrapper.csproj`）中添加：

```xml
<ItemGroup>
  <ProjectReference Include="..\Aspire\Zss.BilliardHall.Wolverine.ServiceDefaults\Zss.BilliardHall.Wolverine.ServiceDefaults.csproj" />
</ItemGroup>
```

### 2. 集成到 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// 一行代码启用所有 ServiceDefaults 功能
builder.AddServiceDefaults();

// 一行代码启用 Wolverine 基础设施
builder.AddWolverineDefaults();

var app = builder.Build();

// 映射健康检查端点（仅开发环境，限制 localhost）
app.MapDefaultEndpoints();

app.Run();
```

---

## 提供的扩展方法

### AddServiceDefaults()

添加完整的 ServiceDefaults 配置，包括：
- OpenTelemetry 配置
- 健康检查
- 服务发现
- HTTP 客户端弹性

**用法**:
```csharp
builder.AddServiceDefaults();
```

### AddWolverineDefaults()

添加 Wolverine 框架配置（命令总线、消息总线、HTTP 端点）。

**用法**:
```csharp
builder.AddWolverineDefaults();
```

**自动包含**:
- 自动扫描入口程序集的 Handler 和 Endpoint（基于约定的路由）
- FluentValidation 集成（自动验证命令和查询）
- 消息队列预留配置点（RabbitMQ、Kafka、Azure Service Bus）

**扩展示例**:
```csharp
// 未来集成 RabbitMQ
builder.Services.AddWolverine(opts =>
{
    opts.UseRabbitMq(rabbitMq =>
    {
        rabbitMq.HostName = "localhost";
        // ...
    });
});
```

**配置示例** (appsettings.json):
```json
{
  "Wolverine": {
    "Messaging": {
      "Provider": "RabbitMQ",
      "ConnectionString": "amqp://guest:guest@localhost:5672"
    }
  }
}
```

### ConfigureOpenTelemetry()

单独配置 OpenTelemetry（日志、指标、追踪）。

**用法**:
```csharp
builder.ConfigureOpenTelemetry();
```

**自动包含**:
- ASP.NET Core 仪器化（HTTP 请求/响应）
- HttpClient 仪器化（出站请求）
- Runtime 仪器化（GC、线程池、异常）

### AddDefaultHealthChecks()

添加默认健康检查（包括 `self` 检查）。

**用法**:
```csharp
builder.AddDefaultHealthChecks();
```

**扩展示例**:
```csharp
builder.AddDefaultHealthChecks();

// 添加数据库检查
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"]);
```

### MapDefaultEndpoints()

映射健康检查端点（仅开发环境，限制 localhost）。

**端点**:
- `/health` - 就绪检查（所有健康检查）
- `/alive` - 存活检查（仅 `live` 标签检查）

**用法**:
```csharp
app.MapDefaultEndpoints();
```

---

## 配置说明

### 服务发现

自动为所有 HttpClient 启用服务发现：

```csharp
// 自动配置（无需手动添加）
services.ConfigureHttpClientDefaults(http =>
{
    http.AddServiceDiscovery();
});

// 使用示例
services.AddHttpClient("MemberService", client =>
{
    client.BaseAddress = new Uri("http+https://members-api"); // 自动解析
});
```

### HTTP 客户端弹性

自动配置标准弹性策略：

| 策略 | 默认配置 |
|------|---------|
| 超时 | 30 秒总超时 |
| 重试 | 3 次，指数退避 |
| 断路器 | 失败率 > 50% 时熔断 |
| 限流 | 100 并发请求 |

**自定义示例**:
```csharp
services.AddHttpClient("PaymentApi")
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.AttemptTimeout.MaxRetryAttempts = 1;
    });
```

### OpenTelemetry Exporter

**开发环境**:
- 自动使用 Aspire Dashboard（无需配置）

**生产环境**:
- 配置环境变量 `OTEL_EXPORTER_OTLP_ENDPOINT`

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

---

## 健康检查端点

### /health（就绪检查）

检查所有注册的健康检查，确保服务可以接收流量。

**响应示例**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy",
      "tags": ["live"]
    },
    "postgres": {
      "status": "Healthy",
      "tags": ["db", "ready"]
    }
  }
}
```

### /alive（存活检查）

仅检查标记为 `live` 的健康检查，用于判断应用是否需要重启。

**响应示例**:
```json
{
  "status": "Healthy",
  "entries": {
    "self": {
      "status": "Healthy",
      "tags": ["live"]
    }
  }
}
```

**安全注意**:
- 仅在开发环境映射（`IsDevelopment()`）
- 限制 localhost 访问（`RequireHost("localhost", "127.0.0.1")`）
- 生产环境需配置认证或使用 Kubernetes 内部探针

---

## 测试

查看 `Zss.BilliardHall.Wolverine.ServiceDefaults.Tests` 项目获取完整测试示例。

**示例测试**:
```csharp
[Fact]
public async Task AddDefaultHealthChecks_ShouldRegisterSelfCheck()
{
    // Arrange
    var builder = WebApplication.CreateBuilder();
    builder.AddDefaultHealthChecks();
    var app = builder.Build();
    
    // Act
    var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
    var result = await healthCheckService.CheckHealthAsync();
    
    // Assert
    result.Status.Should().Be(HealthStatus.Healthy);
    result.Entries.Should().ContainKey("self");
}
```

---

## 最佳实践

### ✅ 推荐

1. **所有服务统一使用 ServiceDefaults**
   ```csharp
   builder.AddServiceDefaults(); // 第一行
   builder.AddWolverineDefaults(); // 第二行
   ```

2. **使用垂直切片架构组织代码**
   ```
   Modules/Members/
     CreateMember/
       CreateMember.cs           # Command
       CreateMemberHandler.cs    # Handler
       CreateMemberEndpoint.cs   # HTTP Endpoint
       CreateMemberValidator.cs  # Validator (可选)
   ```

3. **使用 IHttpClientFactory**
   ```csharp
   services.AddHttpClient<IApiClient, ApiClient>(...);
   ```

4. **添加依赖健康检查**
   ```csharp
   builder.Services.AddHealthChecks().AddNpgSql(...);
   ```

5. **结构化日志**
   ```csharp
   _logger.LogInformation("处理订单 {OrderId}", orderId);
   ```

### ❌ 避免

1. 直接实例化 HttpClient（`new HttpClient()`）
2. 硬编码服务地址（`https://localhost:7001`）
3. 在 ServiceDefaults 中添加业务逻辑
4. 健康检查返回敏感信息

---

## 扩展 ServiceDefaults

只添加**所有服务**都需要的通用功能：

```csharp
/// <summary>
/// 添加公共认证配置
/// </summary>
public static TBuilder AddCommonAuthentication<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
{
    builder.Services.AddAuthentication(...)
        .AddJwtBearer(...);
    
    return builder;
}
```

**注意**:
- ✅ 通用基础设施关注点
- ❌ 特定业务逻辑
- ✅ 稳定、很少变更的配置

---

## 相关文档

- [Aspire 编排架构](../../../doc/03_系统架构设计/Aspire编排架构.md) - Aspire 整体架构
- [ServiceDefaults 集成指南](../../../doc/06_开发规范/ServiceDefaults集成指南.md) - 详细使用说明
- [Aspire 本地开发指南](../../../doc/10_部署与运维/Aspire本地开发指南.md) - 本地运行指南
- [Microsoft Docs](https://aka.ms/dotnet/aspire/service-defaults) - 官方文档

---

## 版本

- **当前版本**: 1.0.0
- **Aspire 版本**: 13.x
- **.NET 版本**: 10.0

---

**最后更新**: 2024-01-15  
**维护者**: 架构团队
