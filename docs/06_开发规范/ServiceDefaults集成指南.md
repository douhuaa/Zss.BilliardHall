# ServiceDefaults 集成指南

## 1. 概述

ServiceDefaults 是 Aspire 应用的共享基础设施配置库，封装了所有服务通用的横切关注点（Cross-Cutting Concerns）。通过一行代码 `builder.AddServiceDefaults()`，即可为服务启用：

- ✅ **服务发现**：自动解析服务名称到实际地址
- ✅ **健康检查**：标准的就绪/存活检查端点
- ✅ **OpenTelemetry**：日志、指标、分布式追踪
- ✅ **HTTP 弹性**：重试、断路器、超时策略
- ✅ **标准化配置**：确保所有服务遵循统一规范

> **目标读者**: 服务开发者、新成员  
> **前置知识**: 基础 ASP.NET Core 开发经验

---

## 2. 快速开始

### 2.1 添加 ServiceDefaults 引用

在服务项目（如 `Bootstrapper.csproj`）中添加项目引用：

```xml
<ItemGroup>
  <ProjectReference Include="..\Aspire\Zss.BilliardHall.Wolverine.ServiceDefaults\Zss.BilliardHall.Wolverine.ServiceDefaults.csproj" />
</ItemGroup>
```

### 2.2 集成到 Program.cs

**最小化集成**（推荐）:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 一行代码启用所有 ServiceDefaults 功能
builder.AddServiceDefaults();

var app = builder.Build();

// 映射健康检查端点（开发环境自动限制 localhost）
app.MapDefaultEndpoints();

app.Run();
```

**完整示例**（包含业务代码）:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. 添加 ServiceDefaults（必须在其他服务注册之前）
builder.AddServiceDefaults();

// 2. 添加业务服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. 添加数据库（连接字符串由 Aspire 自动注入）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("billiard-hall-db")));

var app = builder.Build();

// 4. 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// 5. 映射健康检查端点（必须在最后）
app.MapDefaultEndpoints();

app.Run();
```

---

## 3. 核心功能详解

### 3.1 服务发现（Service Discovery）

#### 3.1.1 自动配置

`AddServiceDefaults()` 自动注册服务发现客户端：

```csharp
// 内部实现（无需手动调用）
builder.Services.AddServiceDiscovery();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddServiceDiscovery(); // 所有 HttpClient 自动启用
});
```

#### 3.1.2 使用服务发现

**方式 1: HttpClient 工厂**（推荐）

```csharp
// 注册命名客户端
services.AddHttpClient("MemberService", client =>
{
    client.BaseAddress = new Uri("http+https://members-api"); // 服务名称
});

// 使用
public class PaymentHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public PaymentHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<Member> GetMemberAsync(Guid memberId)
    {
        var client = _httpClientFactory.CreateClient("MemberService");
        // 自动解析 http+https://members-api → https://localhost:7002
        var response = await client.GetAsync($"/api/members/{memberId}");
        return await response.Content.ReadFromJsonAsync<Member>();
    }
}
```

**方式 2: 类型化客户端**

```csharp
// 定义客户端接口
public interface IMemberApiClient
{
    Task<Member> GetMemberAsync(Guid id);
}

// 实现
public class MemberApiClient : IMemberApiClient
{
    private readonly HttpClient _httpClient;
    
    public MemberApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Member> GetMemberAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Member>($"/api/members/{id}");
    }
}

// 注册（服务发现自动应用）
services.AddHttpClient<IMemberApiClient, MemberApiClient>(client =>
{
    client.BaseAddress = new Uri("http+https://members-api");
});
```

#### 3.1.3 服务名称 Scheme 说明

| Scheme | 行为 | 使用场景 |
|--------|------|---------|
| `http+https://` | 优先 HTTPS，回退 HTTP | **推荐**：本地开发 + 容器 |
| `https://` | 强制 HTTPS | 生产环境，确保加密 |
| `http://` | 强制 HTTP | 内部服务，性能优先 |

### 3.2 健康检查（Health Checks）

#### 3.2.1 默认健康检查

`AddDefaultHealthChecks()` 自动注册 `self` 检查：

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
```

**用途**:
- 验证应用程序基本响应能力
- 被 Kubernetes Liveness Probe 使用

#### 3.2.2 添加依赖健康检查

根据服务依赖添加额外检查：

```csharp
builder.AddDefaultHealthChecks(); // 必须先调用

// 添加 PostgreSQL 检查
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("billiard-hall-db")!,
        name: "postgres",
        tags: ["db", "ready"]  // ready 标签用于就绪检查
    );

// 添加 Redis 检查
builder.Services.AddHealthChecks()
    .AddRedis(
        connectionString: builder.Configuration.GetConnectionString("redis")!,
        name: "redis",
        tags: ["cache", "ready"]
    );
```

**NuGet 包**:
```xml
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.0" />
```

#### 3.2.3 健康检查端点

`MapDefaultEndpoints()` 映射两个端点（仅开发环境，限制 localhost）：

| 端点 | 检查范围 | 用途 |
|------|---------|------|
| `/health` | 所有健康检查（包括依赖） | Readiness Probe（就绪检查） |
| `/alive` | 仅 `live` 标签检查 | Liveness Probe（存活检查） |

**示例响应** (`/health`):

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
      "duration": "00:00:00.0089012",
      "tags": ["db", "ready"]
    }
  }
}
```

#### 3.2.4 自定义健康检查

实现复杂的健康检查逻辑：

```csharp
public class TableAvailabilityHealthCheck : IHealthCheck
{
    private readonly ITableService _tableService;
    
    public TableAvailabilityHealthCheck(ITableService tableService)
    {
        _tableService = tableService;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var availableCount = await _tableService.GetAvailableTableCountAsync(cancellationToken);
            
            if (availableCount == 0)
            {
                return HealthCheckResult.Degraded(
                    description: "所有台球桌已满",
                    data: new Dictionary<string, object> { ["AvailableCount"] = 0 }
                );
            }
            
            return HealthCheckResult.Healthy(
                description: $"{availableCount} 张台球桌可用",
                data: new Dictionary<string, object> { ["AvailableCount"] = availableCount }
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                description: "无法查询台球桌状态",
                exception: ex
            );
        }
    }
}

// 注册
builder.Services.AddHealthChecks()
    .AddCheck<TableAvailabilityHealthCheck>("table-availability", tags: ["business"]);
```

### 3.3 OpenTelemetry 可观测性

#### 3.3.1 自动配置

`ConfigureOpenTelemetry()` 自动配置三大支柱：

**日志（Logging）**:
```csharp
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true; // 包含格式化消息
    logging.IncludeScopes = true;           // 包含日志范围
});
```

**指标（Metrics）**:
```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()  // HTTP 请求计数、延迟
            .AddHttpClientInstrumentation()     // 出站请求
            .AddRuntimeInstrumentation();        // GC、线程池、异常
    });
```

**追踪（Tracing）**:
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource(builder.Environment.ApplicationName) // 应用自定义 Span
            .AddAspNetCoreInstrumentation(options =>
                options.Filter = context =>
                    !context.Request.Path.StartsWithSegments("/health") // 排除健康检查
            )
            .AddHttpClientInstrumentation(); // 出站请求追踪
    });
```

#### 3.3.2 手动创建 Span

使用 `ActivitySource` 创建自定义追踪 Span：

```csharp
public class PaymentService
{
    private static readonly ActivitySource ActivitySource = new("Zss.BilliardHall.Payment");
    
    public async Task ProcessPaymentAsync(Guid orderId, decimal amount)
    {
        // 创建自定义 Span
        using var activity = ActivitySource.StartActivity("ProcessPayment");
        activity?.SetTag("order.id", orderId);
        activity?.SetTag("payment.amount", amount);
        
        try
        {
            // 业务逻辑
            await CallPaymentGatewayAsync(orderId, amount);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}

// 注册 ActivitySource（在 Program.cs 中）
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("Zss.BilliardHall.Payment"); // 注册自定义 Source
    });
```

#### 3.3.3 结构化日志最佳实践

**推荐写法**:

```csharp
// ✅ 使用结构化占位符
_logger.LogInformation("用户 {UserId} 在桌 {TableId} 开始打球，预计费用 {EstimatedCost:F2}",
    userId, tableId, estimatedCost);

// ✅ 使用 LoggerMessage（高性能场景）
private static readonly Action<ILogger, Guid, Guid, decimal, Exception?> _logSessionStarted =
    LoggerMessage.Define<Guid, Guid, decimal>(
        LogLevel.Information,
        new EventId(1001, "SessionStarted"),
        "用户 {UserId} 在桌 {TableId} 开始打球，预计费用 {EstimatedCost:F2}");

_logSessionStarted(_logger, userId, tableId, estimatedCost, null);
```

**避免**:

```csharp
// ❌ 字符串拼接（无结构化）
_logger.LogInformation($"用户 {userId} 在桌 {tableId} 开始打球");

// ❌ 记录敏感信息
_logger.LogInformation("用户密码: {Password}", password); // 禁止！
```

详见：[日志规范](./日志规范.md)

### 3.4 HTTP 客户端弹性

#### 3.4.1 标准弹性策略

`AddStandardResilienceHandler()` 包含以下策略：

| 策略 | 默认配置 | 说明 |
|------|---------|------|
| **超时** | 30 秒总超时，10 秒单次尝试超时 | 防止请求长时间挂起 |
| **重试** | 3 次，指数退避（2s, 4s, 8s） | 瞬时故障自动重试 |
| **断路器** | 失败率 > 50%（10秒窗口）时熔断 30 秒 | 快速失败，避免雪崩 |
| **限流** | 100 并发请求（令牌桶） | 保护下游服务 |

#### 3.4.2 自定义弹性策略

针对特定场景覆盖默认值：

```csharp
services.AddHttpClient("PaymentGateway", client =>
{
    client.BaseAddress = new Uri("http+https://payment-gateway");
})
.AddStandardResilienceHandler(options =>
{
    // 支付网关需要更严格的超时（避免用户等待过久）
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
    
    // 减少重试次数（避免重复扣款）
    options.AttemptTimeout.MaxRetryAttempts = 1;
    
    // 更激进的断路器（快速失败）
    options.CircuitBreaker.FailureRatio = 0.3; // 30% 失败率即熔断
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
});
```

#### 3.4.3 禁用弹性策略

某些场景不需要弹性（如内部管理 API）：

```csharp
services.AddHttpClient("InternalAdmin", client =>
{
    client.BaseAddress = new Uri("http://admin-internal");
})
.RemoveAllLoggers() // 禁用日志（可选）
// 不调用 .AddStandardResilienceHandler()
;
```

---

## 4. 高级配置

### 4.1 限制服务发现 Scheme

生产环境强制 HTTPS：

```csharp
builder.Services.Configure<ServiceDiscoveryOptions>(options =>
{
    options.AllowedSchemes = ["https"]; // 仅允许 HTTPS
});
```

### 4.2 配置 OTLP Exporter

**开发环境**:
- 自动使用 Aspire Dashboard（无需配置）

**生产环境**:

```bash
# 方式 1: 环境变量
export OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317

# 方式 2: appsettings.Production.json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://otel-collector:4317"
}
```

**使用 Azure Monitor**:

```csharp
// 取消 Extensions.cs 中的注释
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry()
       .UseAzureMonitor();
}
```

NuGet 包：
```xml
<PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" Version="1.2.0" />
```

### 4.3 扩展 ServiceDefaults

**场景**: 添加所有服务通用的中间件或服务。

**步骤**:

1. 在 `Extensions.cs` 中添加新扩展方法：

```csharp
/// <summary>
/// 添加公共认证配置（所有服务共享）
/// </summary>
public static TBuilder AddCommonAuthentication<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Auth:Authority"];
            options.Audience = builder.Configuration["Auth:Audience"];
        });
    
    return builder;
}
```

2. 在各服务中使用：

```csharp
builder.AddServiceDefaults();
builder.AddCommonAuthentication(); // 新增
```

**注意事项**:
- ✅ 仅添加**所有服务**都需要的功能
- ❌ 避免添加特定业务逻辑
- ✅ 保持 ServiceDefaults 稳定，频繁变更会影响所有服务

---

## 5. 测试

### 5.1 单元测试 ServiceDefaults

参考现有测试：`Zss.BilliardHall.Wolverine.ServiceDefaults.Tests`

**测试示例**:

```csharp
[Fact]
public async Task AddDefaultHealthChecks_ShouldRegisterSelfCheck()
{
    // Arrange
    var builder = WebApplication.CreateBuilder();
    
    // Act
    builder.AddDefaultHealthChecks();
    var app = builder.Build();
    
    // Assert
    var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
    var result = await healthCheckService.CheckHealthAsync();
    
    result.Status.Should().Be(HealthStatus.Healthy);
    result.Entries.Should().ContainKey("self");
    result.Entries["self"].Tags.Should().Contain("live");
}
```

### 5.2 集成测试健康检查

使用 WebApplicationFactory 测试健康检查端点：

```csharp
public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development"); // 确保映射健康检查端点
        });
    }
    
    [Fact]
    public async Task Health_Endpoint_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.BaseAddress = new Uri("http://localhost"); // 必须是 localhost
        
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
```

---

## 6. 故障排查

### 6.1 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| 健康检查返回 404 | 未调用 `MapDefaultEndpoints()` | 在 `app.MapDefaultEndpoints()` 确认已添加 |
| 健康检查返回 403 | 非 localhost 访问 | 开发环境限制 localhost，生产需配置认证 |
| 服务发现解析失败 | 服务名称错误 | 检查 AppHost 中的资源名称与 URI 是否一致 |
| 无追踪数据 | OTLP 端点未配置 | 开发环境自动，生产需配置 `OTEL_EXPORTER_OTLP_ENDPOINT` |
| HttpClient 弹性策略不生效 | 未使用 HttpClientFactory | 使用 `IHttpClientFactory.CreateClient()` 而非 `new HttpClient()` |

### 6.2 调试技巧

**查看注册的服务**:

```csharp
// 在 Program.cs 启动后
var services = app.Services.GetServices<IHealthCheck>();
foreach (var service in services)
{
    Console.WriteLine($"HealthCheck: {service.GetType().Name}");
}
```

**启用详细日志**:

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.Extensions.ServiceDiscovery": "Debug",
      "Microsoft.Extensions.Http": "Debug",
      "Microsoft.AspNetCore.Diagnostics.HealthChecks": "Debug"
    }
  }
}
```

---

## 7. 最佳实践总结

### 7.1 ✅ 推荐做法

1. **所有服务统一集成 ServiceDefaults**
   ```csharp
   builder.AddServiceDefaults(); // 第一行
   ```

2. **使用类型化 HttpClient**
   ```csharp
   services.AddHttpClient<IMemberApiClient, MemberApiClient>(...)
   ```

3. **添加依赖健康检查**
   ```csharp
   builder.Services.AddHealthChecks().AddNpgSql(...);
   ```

4. **结构化日志**
   ```csharp
   _logger.LogInformation("处理订单 {OrderId}", orderId);
   ```

5. **自定义 Activity Span**
   ```csharp
   using var activity = ActivitySource.StartActivity("PaymentProcessing");
   ```

### 7.2 ❌ 避免做法

1. **直接 `new HttpClient()`**（绕过弹性和服务发现）
2. **硬编码服务地址**（如 `https://localhost:7001`）
3. **在 ServiceDefaults 中添加业务逻辑**
4. **生产环境不配置 OTLP Exporter**（丢失遥测数据）
5. **健康检查返回敏感信息**（如数据库连接字符串）

---

## 8. 检查清单

新服务集成 ServiceDefaults 前的检查：

- [ ] 添加 `Zss.BilliardHall.Wolverine.ServiceDefaults` 项目引用
- [ ] `Program.cs` 中调用 `builder.AddServiceDefaults()`
- [ ] `Program.cs` 中调用 `app.MapDefaultEndpoints()`
- [ ] 数据库连接使用 `GetConnectionString()`（由 Aspire 注入）
- [ ] HttpClient 使用 `IHttpClientFactory` 而非直接实例化
- [ ] 添加依赖的健康检查（数据库、缓存等）
- [ ] 结构化日志使用占位符（`{PropertyName}`）
- [ ] 避免记录敏感信息（密码、Token、PII）
- [ ] 编写健康检查集成测试
- [ ] 在 Aspire Dashboard 中验证遥测数据

---

## 9. 扩展阅读

- [Aspire 编排架构](../03_系统架构设计/Aspire编排架构.md) - Aspire 整体架构
- [Aspire 本地开发指南](../10_部署与运维/Aspire本地开发指南.md) - 本地运行指南
- [日志规范](./日志规范.md) - 结构化日志最佳实践
- [Microsoft Docs - Service Defaults](https://aka.ms/dotnet/aspire/service-defaults)
- [Microsoft Docs - Health Checks](https://aka.ms/dotnet/aspire/healthchecks)

---

## 10. 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0.0 | 2024-01-15 | 初始版本，涵盖 ServiceDefaults 核心功能和最佳实践 | 架构团队 |

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
