# Bootstrapper 服务

Bootstrapper 是基于 Wolverine 框架构建的微服务，实现了垂直切片架构（Vertical Slice Architecture）。

## 架构特点

- **Wolverine 框架**: 使用 Wolverine 进行消息处理和 HTTP 端点路由
- **垂直切片架构**: 按业务功能组织代码，而非传统的分层架构
- **Marten 文档数据库**: 使用 Marten 作为事件存储和文档数据库
- **Aspire ServiceDefaults 集成**: 完全集成 Aspire 服务默认配置

## Aspire ServiceDefaults 集成

Bootstrapper 通过引用 `Zss.BilliardHall.ServiceDefaults` 项目获得以下功能：

### 1. 服务发现 (Service Discovery)
- 自动注册和发现其他服务
- HTTP 客户端自动配置服务发现
- 支持跨服务通信

### 2. 弹性处理 (Resilience)
- 标准的重试策略
- 熔断器模式
- 超时处理
- 通过 `Microsoft.Extensions.Http.Resilience` 实现

### 3. 健康检查 (Health Checks)
开发环境下提供以下端点：
- `/health`: 所有健康检查必须通过
- `/alive`: 仅需 "live" 标签的健康检查通过

### 4. OpenTelemetry 遥测
自动收集和导出：
- **日志**: 包含格式化消息和作用域信息
- **指标**:
  - ASP.NET Core 请求指标
  - HTTP 客户端指标
  - .NET 运行时指标
- **追踪**:
  - ASP.NET Core 请求追踪
  - HTTP 客户端追踪
  - 自动排除健康检查端点的追踪

## 使用方式

在 `Program.cs` 中通过以下方式集成 ServiceDefaults：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 Aspire 服务默认配置
builder.AddServiceDefaults();

var app = builder.Build();

// 映射默认端点（健康检查等）
app.MapDefaultEndpoints();

// 其他配置...
app.Run();
```

## 配置

### OpenTelemetry 导出器
默认情况下，如果设置了 `OTEL_EXPORTER_OTLP_ENDPOINT` 环境变量，遥测数据将通过 OTLP 协议导出。

### 服务发现
HTTP 客户端会自动配置服务发现。如需限制允许的协议，可以取消注释 `ServiceDefaults/Extensions.cs` 中的相关配置。

## 依赖项

- `Marten.AspNetCore`: Marten 文档数据库和事件存储
- `Serilog.AspNetCore`: 结构化日志
- `WolverineFx.Http.FluentValidation`: Wolverine HTTP 支持和 FluentValidation 集成
- `Zss.BilliardHall.ServiceDefaults`: Aspire 服务默认配置

## 运行

### 独立运行
```bash
cd src/Wolverine/Bootstrapper
dotnet run
```

### 通过 Aspire AppHost 运行
```bash
cd src/Zss.BilliardHall/Zss.BilliardHall.AppHost
dotnet run
```

通过 AppHost 运行时，Bootstrapper 会自动：
- 连接到 PostgreSQL 数据库
- 注册到 Aspire Dashboard
- 启用服务发现和遥测

## 监控

在开发环境中，访问以下端点查看服务状态：
- `http://localhost:5239/health`: 健康检查
- `http://localhost:5239/alive`: 存活检查

通过 Aspire Dashboard 可以查看：
- 服务状态和日志
- 分布式追踪
- 性能指标
- 服务间依赖关系
