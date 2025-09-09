# .NET Aspire 编排模式 (Aspire Orchestration Patterns)

## 总体架构 (Overall Architecture)

.NET Aspire 提供了云原生应用的编排、配置和监控能力：
- **AppHost**: 应用编排和服务发现
- **ServiceDefaults**: 共享服务配置（健康检查、OpenTelemetry、弹性处理）
- **DistributedApplication**: 分布式应用管理
- **资源管理**: Redis、MySQL、监控工具等基础设施

## AppHost 编排模式

### 1. 基础应用编排

```csharp
// AppHost.cs - 应用编排入口
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// 基础设施服务定义
var redis = builder
    .AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)  // 持久化容器生命周期
    .WithDataVolume("redis-data")               // 数据卷持久化
    .WithRedisCommander();                      // 开发工具（仅开发环境）

var mysql = builder
    .AddMySql("mysql", password: "MyPassword123!")
    .WithDataVolume("mysql-data")               // MySQL 数据持久化
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("MYSQL_ROOT_PASSWORD", "MyPassword123!")
    .WithEnvironment("MYSQL_DATABASE", "BilliardHall");

// 创建数据库实例
var billiardHallDb = mysql.AddDatabase("BilliardHallDb", "BilliardHall");

// 应用服务编排
var blazorApp = builder
    .AddProject<Zss_BilliardHall_Blazor>("billiard-hall-blazor")
    .WithReference(redis, "Redis")              // Redis 连接字符串
    .WithReference(billiardHallDb, "Default")   // 主数据库连接
    .WaitFor(redis)                             // 启动依赖
    .WaitFor(billiardHallDb)
    .WithReplicas(2)                            // 水平扩展
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// 数据库迁移服务
var migrator = builder
    .AddProject<Zss_BilliardHall_DbMigrator>("billiard-hall-migrator")
    .WithReference(billiardHallDb, "Default")
    .WaitFor(billiardHallDb);

// 开发环境特定配置
if (builder.Environment.IsDevelopment())
{
    // 添加管理工具
    redis.WithRedisCommander(c => c.WithHostPort(8081));
    mysql.WithPhpMyAdmin(c => c.WithHostPort(8082));
    
    // 开发数据种子
    builder.AddProject<Zss_BilliardHall_DataSeeder>("data-seeder")
           .WithReference(billiardHallDb, "Default")
           .WaitFor(migrator);
}

// 生产环境特定配置
if (builder.Environment.IsProduction())
{
    blazorApp.WithReplicas(5)  // 生产环境更多副本
             .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production");
    
    // 添加负载均衡器
    var loadBalancer = builder.AddContainer("nginx-lb", "nginx:alpine")
                             .WithBindMount("./nginx.conf", "/etc/nginx/nginx.conf")
                             .WithHttpEndpoint(port: 80, targetPort: 80)
                             .WaitFor(blazorApp);
}

// 构建并运行应用
builder.Build().Run();
```

### 2. 高级资源配置

```csharp
// 复杂的基础设施配置
public static class InfrastructureExtensions
{
    public static IDistributedApplicationBuilder AddBilliardHallInfrastructure(
        this IDistributedApplicationBuilder builder)
    {
        // Redis 集群配置
        var redisCluster = builder.AddRedis("redis")
            .WithRedisInsight()  // Redis 监控工具
            .WithDataVolume()
            .WithLifetime(ContainerLifetime.Persistent);

        // MySQL 主从配置
        var mysqlMaster = builder.AddMySql("mysql-master")
            .WithDataVolume("mysql-master-data")
            .WithEnvironment("MYSQL_REPLICATION_MODE", "master")
            .WithEnvironment("MYSQL_REPLICATION_USER", "replicator")
            .WithEnvironment("MYSQL_REPLICATION_PASSWORD", "ReplicatorPass123!");

        var mysqlSlave = builder.AddMySql("mysql-slave")
            .WithDataVolume("mysql-slave-data")
            .WithEnvironment("MYSQL_REPLICATION_MODE", "slave")
            .WithEnvironment("MYSQL_MASTER_HOST", "mysql-master")
            .WithEnvironment("MYSQL_REPLICATION_USER", "replicator")
            .WithEnvironment("MYSQL_REPLICATION_PASSWORD", "ReplicatorPass123!")
            .WaitFor(mysqlMaster);

        // Elasticsearch 用于日志聚合
        var elasticsearch = builder.AddElasticsearch("elasticsearch")
            .WithDataVolume("elasticsearch-data")
            .WithLifetime(ContainerLifetime.Persistent);

        // Kibana 用于日志可视化
        var kibana = builder.AddKibana("kibana")
            .WithReference(elasticsearch)
            .WithHostPort(5601);

        // Jaeger 用于分布式追踪
        var jaeger = builder.AddJaeger("jaeger")
            .WithHostPort(16686)  // Jaeger UI
            .WithOtlpEndpoint();  // OTLP 端点

        return builder;
    }
}

// 使用扩展方法
var builder = DistributedApplication.CreateBuilder(args);
builder.AddBilliardHallInfrastructure();
```

### 3. 环境特定配置

```csharp
// EnvironmentConfiguration.cs
public static class EnvironmentConfiguration
{
    public static void ConfigureDevelopment(IDistributedApplicationBuilder builder)
    {
        builder.AddParameter("dev-connection-string")
               .WithDefault("Server=localhost;Database=BilliardHall_Dev;...")
               .WithSecret(); // 标记为敏感信息
        
        // 开发环境使用本地数据库
        builder.AddSqlServer("sqlserver-dev")
               .WithHostPort(1433);
    }

    public static void ConfigureStaging(IDistributedApplicationBuilder builder)
    {
        // 使用 Azure 资源
        builder.AddAzureSqlDatabase("azure-sql-staging")
               .WithConnectionString("staging-sql-connection");
        
        builder.AddAzureRedis("azure-redis-staging")
               .WithConnectionString("staging-redis-connection");
    }

    public static void ConfigureProduction(IDistributedApplicationBuilder builder)
    {
        // 生产环境配置
        builder.AddAzureSqlDatabase("azure-sql-prod")
               .WithConnectionString("prod-sql-connection");
        
        builder.AddAzureRedis("azure-redis-prod")
               .WithConnectionString("prod-redis-connection");
        
        // 添加应用洞察
        builder.AddAzureApplicationInsights("app-insights")
               .WithConnectionString("app-insights-connection");
    }
}

// 在 AppHost 中使用
var builder = DistributedApplication.CreateBuilder(args);

switch (builder.Environment.EnvironmentName)
{
    case "Development":
        EnvironmentConfiguration.ConfigureDevelopment(builder);
        break;
    case "Staging":
        EnvironmentConfiguration.ConfigureStaging(builder);
        break;
    case "Production":
        EnvironmentConfiguration.ConfigureProduction(builder);
        break;
}
```

## ServiceDefaults 配置模式

### 1. 基础服务默认配置

```csharp
// Extensions.cs - ServiceDefaults
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.AddServiceDiscovery();
        builder.ConfigureHttpClientDefaults();
        builder.AddRedisDefaults();
        builder.AddDatabaseDefaults();
        
        return builder;
    }

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()  // EF Core 指标
                       .AddRedisInstrumentation();               // Redis 指标
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                       .AddAspNetCoreInstrumentation(options =>
                       {
                           options.RecordException = true;
                           options.Filter = context => 
                               !context.Request.Path.StartsWithSegments("/health") &&
                               !context.Request.Path.StartsWithSegments("/alive");
                       })
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()  // EF Core 追踪
                       .AddRedisInstrumentation();               // Redis 追踪
            });

        builder.AddOpenTelemetryExporters();
        return builder;
    }

    private static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
            .AddDbContextCheck<BilliardHallDbContext>() // 数据库健康检查
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!) // Redis 健康检查
            .AddUrlGroup(new Uri("https://www.google.com"), "external", HealthStatus.Degraded); // 外部依赖检查

        return builder;
    }

    private static TBuilder AddServiceDiscovery<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddServiceDiscovery();
        builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        {
            options.AllowedSchemes = ["https", "http"];
        });
        
        return builder;
    }
}
```

### 2. HTTP 客户端配置

```csharp
public static class HttpClientExtensions
{
    public static TBuilder ConfigureHttpClientDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // 添加标准弹性处理
            http.AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                options.Timeout.TotalRequestTimeout = TimeSpan.FromSeconds(30);
            });

            // 添加服务发现
            http.AddServiceDiscovery();

            // 添加日志记录
            http.AddLogger();

            // 添加自定义处理器
            http.AddHttpMessageHandler<CorrelationIdHandler>();
            http.AddHttpMessageHandler<RequestLoggingHandler>();
        });

        return builder;
    }
}

// 自定义 HTTP 处理器
public class CorrelationIdHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("X-Correlation-ID"))
        {
            request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public class RequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<RequestLoggingHandler> _logger;

    public RequestLoggingHandler(ILogger<RequestLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending HTTP request: {Method} {Uri}", 
                               request.Method, request.RequestUri);

        var stopwatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation("Received HTTP response: {StatusCode} in {ElapsedMilliseconds}ms",
                               (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

### 3. 数据库和缓存配置

```csharp
public static class DataAccessExtensions
{
    public static TBuilder AddDatabaseDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        // 添加数据库上下文
        builder.Services.AddDbContext<BilliardHallDbContext>((services, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                
                mysqlOptions.CommandTimeout(30);
            });

            // 开发环境启用敏感数据日志
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // 添加拦截器
            options.AddInterceptors(services.GetRequiredService<AuditingInterceptor>());
        });

        return builder;
    }

    public static TBuilder AddRedisDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "BilliardHall";
        });

        // 添加分布式锁
        builder.Services.AddSingleton<IDistributedLockProvider>(provider =>
        {
            var redis = ConnectionMultiplexer.Connect(
                builder.Configuration.GetConnectionString("Redis")!);
            return new RedisDistributedLockProvider(redis.GetDatabase());
        });

        return builder;
    }
}

// 审计拦截器
public class AuditingInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<IAuditedObject>();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreationTime = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModificationTime = DateTime.UtcNow;
                    break;
            }
        }
    }
}
```

## 监控和可观测性模式

### 1. 自定义指标

```csharp
// BilliardHallMetrics.cs
public static class BilliardHallMetrics
{
    private static readonly Counter<long> TablesReserved = 
        Meter.CreateCounter<long>("billiard_tables_reserved", "reservations", "Number of table reservations");
    
    private static readonly Histogram<double> ReservationDuration = 
        Meter.CreateHistogram<double>("reservation_duration", "minutes", "Duration of reservations");
    
    private static readonly ObservableGauge<int> AvailableTables = 
        Meter.CreateObservableGauge<int>("available_tables", "tables", "Number of available tables");

    public static readonly Meter Meter = new("BilliardHall", "1.0.0");

    public static void RecordTableReservation(string tableType, string status)
    {
        TablesReserved.Add(1, 
            new KeyValuePair<string, object?>("table_type", tableType),
            new KeyValuePair<string, object?>("status", status));
    }

    public static void RecordReservationDuration(double minutes, string tableType)
    {
        ReservationDuration.Record(minutes,
            new KeyValuePair<string, object?>("table_type", tableType));
    }
}

// 在 Application Service 中使用
public class BilliardTableAppService : BilliardHallAppService
{
    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto input)
    {
        var reservation = await _reservationManager.CreateAsync(input);
        
        // 记录指标
        BilliardHallMetrics.RecordTableReservation(
            reservation.Table.Type.ToString(), 
            reservation.Status.ToString());
        
        return await MapToGetOutputDtoAsync(reservation);
    }
}
```

### 2. 结构化日志

```csharp
// StructuredLogging.cs
public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Table {TableId} reserved by user {UserId} for {Duration} minutes")]
    public static partial void TableReserved(
        this ILogger logger, 
        Guid tableId, 
        Guid userId, 
        int duration);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Table {TableId} reservation failed: {Reason}")]
    public static partial void TableReservationFailed(
        this ILogger logger, 
        Guid tableId, 
        string reason);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Database operation failed for table {TableId}")]
    public static partial void DatabaseOperationFailed(
        this ILogger logger, 
        Guid tableId, 
        Exception ex);
}

// 在服务中使用
public class BilliardTableAppService : BilliardHallAppService
{
    private readonly ILogger<BilliardTableAppService> _logger;

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto input)
    {
        try
        {
            var reservation = await _reservationManager.CreateAsync(input);
            
            _logger.TableReserved(
                input.TableId, 
                CurrentUser.Id!.Value, 
                input.DurationMinutes);
            
            return await MapToGetOutputDtoAsync(reservation);
        }
        catch (BusinessException ex)
        {
            _logger.TableReservationFailed(input.TableId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.DatabaseOperationFailed(input.TableId, ex);
            throw;
        }
    }
}
```

### 3. 健康检查扩展

```csharp
// BilliardHallHealthChecks.cs
public class BilliardTableHealthCheck : IHealthCheck
{
    private readonly IBilliardTableRepository _repository;

    public BilliardTableHealthCheck(IBilliardTableRepository repository)
    {
        _repository = repository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var availableTables = await _repository.CountAsync(t => t.Status == BilliardTableStatus.Available);
            var totalTables = await _repository.CountAsync();

            var data = new Dictionary<string, object>
            {
                ["available_tables"] = availableTables,
                ["total_tables"] = totalTables,
                ["availability_percentage"] = totalTables > 0 ? (double)availableTables / totalTables * 100 : 0
            };

            if (availableTables == 0)
            {
                return HealthCheckResult.Degraded("No tables available", data: data);
            }

            return HealthCheckResult.Healthy("Tables available", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cannot check table availability", ex);
        }
    }
}

// 注册健康检查
builder.Services.AddHealthChecks()
    .AddCheck<BilliardTableHealthCheck>("billiard_tables")
    .AddTypeActivatedCheck<DatabaseConnectionHealthCheck>(
        "database_connection",
        args: new object[] { builder.Configuration.GetConnectionString("Default")! });
```

## 部署和扩展模式

### 1. 容器化配置

```dockerfile
# Dockerfile - Blazor 应用
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Zss.BilliardHall.Blazor/Zss.BilliardHall.Blazor.csproj", "src/Zss.BilliardHall.Blazor/"]
COPY ["src/Zss.BilliardHall.ServiceDefaults/Zss.BilliardHall.ServiceDefaults.csproj", "src/Zss.BilliardHall.ServiceDefaults/"]

RUN dotnet restore "src/Zss.BilliardHall.Blazor/Zss.BilliardHall.Blazor.csproj"
COPY . .

WORKDIR "/src/src/Zss.BilliardHall.Blazor"
RUN dotnet build "Zss.BilliardHall.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zss.BilliardHall.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "Zss.BilliardHall.Blazor.dll"]
```

### 2. Kubernetes 部署清单

```yaml
# k8s-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: billiard-hall-blazor
  labels:
    app: billiard-hall-blazor
spec:
  replicas: 3
  selector:
    matchLabels:
      app: billiard-hall-blazor
  template:
    metadata:
      labels:
        app: billiard-hall-blazor
    spec:
      containers:
      - name: blazor-app
        image: billiard-hall-blazor:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__Default
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        - name: ConnectionStrings__Redis
          valueFrom:
            secretKeyRef:
              name: redis-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: billiard-hall-blazor-service
spec:
  selector:
    app: billiard-hall-blazor
  ports:
    - protocol: TCP
      port: 80
      targetPort: 80
  type: LoadBalancer
```

这些 Aspire 模式确保了应用的可观测性、弹性和可扩展性，为云原生环境提供了完整的解决方案。