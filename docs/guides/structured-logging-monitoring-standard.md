# 结构化日志与监控工程标准

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以 [ADR-340](../adr/technical/ADR-340-structured-logging-monitoring-constraints.md) 和 [ADR-350](../adr/technical/ADR-350-logging-observability-standards.md) 为准。

**性质**：工程标准（Engineering Standard）- 非裁决性  
**版本**：1.0  
**最后更新**：2026-01-24  
**状态**：Active  
**相关 ADR**：
- [ADR-340](../adr/technical/ADR-340-structured-logging-monitoring-constraints.md)（裁决性规则）
- [ADR-350](../adr/technical/ADR-350-logging-observability-standards.md)（裁决性规则）

---

## 文档定位

> **本文档不具裁决力，不可作为架构违规判定依据。**

本文档提供结构化日志和监控的工程实践指导，包括 Serilog 配置、OpenTelemetry 设置、日志字段推荐、日志级别使用、监控指标和告警建议等。

本文档内容随工程实践演进而更新，不受 ADR 变更流程约束。

---

## 目录

1. [Serilog 结构化日志配置](#一serilog-结构化日志配置)
2. [OpenTelemetry 追踪和指标配置](#二opentelemetry-追踪和指标配置)
3. [日志字段推荐](#三日志字段推荐)
4. [日志级别使用指南](#四日志级别使用指南)
5. [监控指标和告警建议](#五监控指标和告警建议)
6. [常见问题解答](#六常见问题解答)

---

## 一、Serilog 结构化日志配置

### 1.1 基础配置

**推荐的 PlatformBootstrapper 配置**：

```csharp
public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "Zss.BilliardHall")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();
    }
}
```

**Host 集成**：

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 替换默认日志
builder.Host.UseSerilog();

PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);
// ... 其他配置
```

### 1.2 开发环境 vs 生产环境

**开发环境**（`appsettings.Development.json`）：

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

**生产环境**（`appsettings.Production.json`）：

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 1.3 推荐的 Sinks

| Sink | 用途 | 推荐场景 |
|------|------|---------|
| **Console** | 控制台输出 | 开发环境、Docker 容器 |
| **File** | 文件输出 | 本地开发、传统部署 |
| **Seq** | 结构化日志服务器 | 开发和测试环境 |
| **Elasticsearch** | 日志聚合和搜索 | 生产环境 |
| **ApplicationInsights** | Azure 监控 | Azure 部署 |

---

## 二、OpenTelemetry 追踪和指标配置

### 2.1 基础配置

**推荐的 PlatformBootstrapper 配置**：

```csharp
public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // ... Serilog 配置 ...

        // 配置 OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: "Zss.BilliardHall",
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.request.path", httpRequest.Path);
                        activity.SetTag("http.request.method", httpRequest.Method);
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                    };
                })
                .AddHttpClientInstrumentation()
                .AddSource("Wolverine") // Wolverine 消息总线追踪
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }));
    }
}
```

### 2.2 配置文件

**appsettings.json**：

```json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "ServiceName": "Zss.BilliardHall",
    "ServiceVersion": "1.0.0"
  }
}
```

### 2.3 开发环境本地测试

**使用 Docker Compose 启动本地 OTLP Collector**：

```yaml
version: '3.8'
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "4317:4317"  # OTLP gRPC
      - "16686:16686" # Jaeger UI
    environment:
      - COLLECTOR_OTLP_ENABLED=true
```

---

## 三、日志字段推荐

### 3.1 必须包含的上下文字段

所有业务日志**应该**包含以下上下文字段：

| 字段名 | 类型 | 说明 | 示例 |
|--------|------|------|------|
| **CorrelationId** | Guid | 请求关联 ID | `a1b2c3d4-...` |
| **UserId** / **MemberId** | Guid | 当前用户/会员 ID | `m-12345...` |
| **TenantId** | Guid | 多租户场景的租户 ID | `t-98765...` |
| **CommandId** / **QueryId** / **EventId** | Guid | 消息唯一标识 | `cmd-abc123...` |
| **HandlerName** | string | Handler 类型名称 | `CreateOrderHandler` |
| **Module** | string | 模块名称 | `Orders` |

### 3.2 日志消息模板推荐

**Command Handler**：

```csharp
_logger.LogInformation(
    "开始处理命令 {CommandName}，命令ID {CommandId}，会员ID {MemberId}",
    nameof(CreateOrder), command.CommandId, command.MemberId);

// ... 处理逻辑 ...

_logger.LogInformation(
    "命令处理成功 {CommandName}，命令ID {CommandId}，结果ID {ResultId}，耗时 {ElapsedMs}ms",
    nameof(CreateOrder), command.CommandId, orderId, stopwatch.ElapsedMilliseconds);
```

**Query Handler**：

```csharp
_logger.LogInformation(
    "开始查询 {QueryName}，查询ID {QueryId}，参数 {@QueryParams}",
    nameof(GetOrderById), query.QueryId, new { query.OrderId });

// ... 查询逻辑 ...

_logger.LogInformation(
    "查询完成 {QueryName}，查询ID {QueryId}，结果数量 {ResultCount}，耗时 {ElapsedMs}ms",
    nameof(GetOrderById), query.QueryId, results.Count, stopwatch.ElapsedMilliseconds);
```

**Event Handler**：

```csharp
_logger.LogInformation(
    "接收到事件 {EventName}，事件ID {EventId}，聚合ID {AggregateId}",
    nameof(OrderCreated), @event.EventId, @event.OrderId);

// ... 处理逻辑 ...

_logger.LogInformation(
    "事件处理完成 {EventName}，事件ID {EventId}，耗时 {ElapsedMs}ms",
    nameof(OrderCreated), @event.EventId, stopwatch.ElapsedMilliseconds);
```

### 3.3 避免敏感信息泄露

**禁止记录的敏感信息**：

- ❌ 密码、密钥、Token
- ❌ 完整的银行卡号、身份证号
- ❌ 个人隐私信息（除非已脱敏）

**推荐脱敏方式**：

```csharp
// ✅ 正确：脱敏处理
_logger.LogInformation(
    "会员登录成功，手机号 {PhoneNumber}",
    MaskPhoneNumber(member.PhoneNumber)); // 137****5678

// ❌ 错误：直接记录敏感信息
_logger.LogInformation(
    "会员登录成功，手机号 {PhoneNumber}",
    member.PhoneNumber); // 13712345678
```

---

## 四、日志级别使用指南

### 4.1 日志级别定义

| 级别 | 用途 | 典型场景 |
|------|------|---------|
| **Trace** | 详细的调试信息 | 方法进入/退出、循环迭代 |
| **Debug** | 调试信息 | 变量值、中间状态 |
| **Information** | 关键业务事件 | 订单创建、支付成功 |
| **Warning** | 警告但不影响运行 | 配置缺失使用默认值、重试成功 |
| **Error** | 错误但系统可恢复 | 业务异常、验证失败 |
| **Critical** | 严重错误需立即处理 | 系统崩溃、数据损坏 |

### 4.2 使用建议

**Information** - 记录业务里程碑：

```csharp
// ✅ 正确：关键业务事件
_logger.LogInformation("订单 {OrderId} 创建成功", orderId);
_logger.LogInformation("会员 {MemberId} 支付完成，金额 {Amount}", memberId, amount);
```

**Warning** - 记录潜在问题：

```csharp
// ✅ 正确：潜在问题但不影响功能
_logger.LogWarning("库存不足，商品 {ProductId}，当前库存 {Stock}", productId, stock);
_logger.LogWarning("HTTP 请求重试 {RetryCount} 次后成功", retryCount);
```

**Error** - 记录业务异常：

```csharp
// ✅ 正确：业务规则违反
_logger.LogError(ex, "订单创建失败，订单ID {OrderId}，原因 {Reason}", orderId, ex.Message);
_logger.LogError("支付失败，会员 {MemberId}，订单 {OrderId}", memberId, orderId);
```

**Critical** - 记录系统级错误：

```csharp
// ✅ 正确：系统级故障
_logger.LogCritical(ex, "数据库连接池耗尽，无法创建新连接");
_logger.LogCritical("消息队列消费者意外停止，队列名 {QueueName}", queueName);
```

### 4.3 避免的反模式

```csharp
// ❌ 错误：过度使用 Information
_logger.LogInformation("进入方法 CreateOrder"); // 应该用 Trace 或不记录

// ❌ 错误：日志级别不匹配
_logger.LogError("订单创建成功"); // 成功不应该用 Error

// ❌ 错误：噪音日志
for (var i = 0; i < 10000; i++)
{
    _logger.LogInformation("处理第 {Index} 项", i); // 循环中大量日志
}
```

---

## 五、监控指标和告警建议

### 5.1 推荐的业务指标

**订单模块**：

```csharp
// 在 Handler 中使用 ActivitySource 记录自定义指标
private static readonly ActivitySource ActivitySource = new("Zss.BilliardHall.Orders");

public async Task<Guid> Handle(CreateOrder command)
{
    using var activity = ActivitySource.StartActivity("CreateOrder");
    activity?.SetTag("order.member_id", command.MemberId);
    activity?.SetTag("order.items_count", command.Items.Count);
    
    // ... 业务逻辑 ...
    
    activity?.SetTag("order.total_amount", order.TotalAmount);
    activity?.SetTag("order.status", "Created");
    
    return order.Id;
}
```

**推荐的业务指标**：

| 指标名 | 类型 | 说明 |
|--------|------|------|
| `order.created.count` | Counter | 订单创建总数 |
| `order.created.amount` | Histogram | 订单金额分布 |
| `payment.success.rate` | Gauge | 支付成功率 |
| `handler.execution.duration` | Histogram | Handler 执行耗时 |

### 5.2 推荐的技术指标

**已自动收集**（通过 OpenTelemetry）：

- HTTP 请求数、延迟、错误率
- 数据库查询数、延迟
- .NET Runtime 指标（GC、线程池、异常）

**推荐告警阈值**：

| 指标 | 阈值 | 严重性 |
|------|------|--------|
| HTTP 错误率 | > 5% | Warning |
| HTTP P99 延迟 | > 1000ms | Warning |
| 数据库连接池耗尽 | 1 次 | Critical |
| Handler 异常率 | > 10% | Error |
| GC Pause Time | > 100ms | Warning |

### 5.3 追踪和 Span

**自定义 Span**：

```csharp
using var activity = ActivitySource.StartActivity("ValidateOrder");
activity?.SetTag("order.id", orderId);
activity?.SetTag("validation.rules_count", rules.Count);

try
{
    // ... 验证逻辑 ...
    activity?.SetTag("validation.result", "Success");
}
catch (ValidationException ex)
{
    activity?.SetTag("validation.result", "Failed");
    activity?.SetTag("validation.error_count", ex.Errors.Count);
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

---

## 六、常见问题解答

### Q1：何时使用结构化日志 vs 普通日志？

**A**：始终使用结构化日志。结构化日志允许：

- 高效搜索和过滤
- 自动化分析和告警
- 仪表板和可视化

```csharp
// ✅ 正确：结构化
_logger.LogInformation("订单 {OrderId} 创建，金额 {Amount}", orderId, amount);

// ❌ 错误：非结构化
_logger.LogInformation($"订单 {orderId} 创建，金额 {amount}");
```

### Q2：如何在不同模块间传递 CorrelationId？

**A**：使用 `ILoggerFactory` 创建 LogContext：

```csharp
// 在 HTTP 中间件或消息总线中设置
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    // 该范围内的所有日志自动包含 CorrelationId
    await next();
}
```

### Q3：日志量太大怎么办？

**A**：调整日志级别和采样：

1. 生产环境使用 `Information` 级别
2. 为高频操作使用采样策略
3. 使用条件日志：

```csharp
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("详细调试信息 {@Data}", expensiveData);
}
```

### Q4：如何测试日志记录？

**A**：使用 `ILogger` 的测试替身：

```csharp
// 使用 Moq
var loggerMock = new Mock<ILogger<CreateOrderHandler>>();

var handler = new CreateOrderHandler(loggerMock.Object, ...);
await handler.Handle(command);

// 验证日志调用
loggerMock.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("订单创建成功")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

### Q5：如何处理性能敏感的代码中的日志？

**A**：

1. 使用 `LoggerMessage` 源生成器（.NET 6+）：

```csharp
public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "订单 {OrderId} 创建成功，金额 {Amount}")]
    public static partial void LogOrderCreated(
        this ILogger logger, Guid orderId, decimal amount);
}

// 使用
_logger.LogOrderCreated(orderId, amount);
```

2. 避免在高频循环中记录日志
3. 使用异步 Sinks 减少阻塞

---

## 附录 A：完整配置示例

### PlatformBootstrapper.cs

```csharp
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace Zss.BilliardHall.Platform;

public static class PlatformBootstrapper
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ConfigureSerilog(configuration, environment);
        ConfigureOpenTelemetry(services, configuration, environment);
    }

    private static void ConfigureSerilog(IConfiguration configuration, IHostEnvironment environment)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "Zss.BilliardHall")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void ConfigureOpenTelemetry(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: "Zss.BilliardHall",
                    serviceVersion: typeof(PlatformBootstrapper).Assembly.GetName().Version?.ToString() ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }));
    }
}
```

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "ServiceName": "Zss.BilliardHall",
    "ServiceVersion": "1.0.0"
  }
}
```

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-24 | 初始版本，定义结构化日志和监控工程标准 | @copilot |

---

**相关文档**：

- [ADR-340：结构化日志与监控约束](../adr/technical/ADR-340-structured-logging-monitoring-constraints.md)（裁决性）
- [ADR-0002：Platform/Application/Host 启动体系](../adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [Serilog 官方文档](https://serilog.net/)
- [OpenTelemetry .NET 文档](https://opentelemetry.io/docs/languages/net/)
