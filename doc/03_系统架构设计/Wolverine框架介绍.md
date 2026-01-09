# Wolverine 框架介绍

## 1. 概述

Wolverine 是一个现代化的 .NET 应用框架，专注于简化应用程序的消息处理、命令处理和后台任务。它是 [JasperFx](https://github.com/JasperFx) 生态系统的一部分，与 Marten 紧密集成。

**官方网站**: https://wolverine.netlify.app/

## 2. 核心特性

### 2.1 基于约定的命令/查询处理

Wolverine 使用约定优于配置的理念，自动发现和注册处理器：

```csharp
// 定义命令
public record CreateMemberCommand(
    string Name,
    string Phone,
    string Email
);

// 定义处理器 - 无需接口或基类
public class CreateMemberHandler
{
    // Wolverine 自动注入依赖
    public async Task<MemberCreated> Handle(
        CreateMemberCommand command,
        IDocumentSession session,
        ILogger<CreateMemberHandler> logger,
        CancellationToken ct)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone,
            Email = command.Email,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(member);
        await session.SaveChangesAsync(ct);

        logger.LogInformation("创建会员成功: {MemberId}", member.Id);

        return new MemberCreated(member.Id);
    }
}
```

**约定规则**:
- 方法名必须是 `Handle` 或 `HandleAsync`
- 第一个参数是消息/命令/查询
- 其他参数由 Wolverine 自动注入（服务、上下文等）
- 返回值可以是 `void`、`Task`、结果对象或事件

### 2.2 内置消息总线

Wolverine 提供了轻量级的消息总线，支持进程内和跨进程通信：

```csharp
// 发送命令（同步等待结果）
var result = await _bus.InvokeAsync<MemberCreated>(
    new CreateMemberCommand("张三", "13800138000", "zhang@example.com")
);

// 发布事件（异步，不等待）
await _bus.PublishAsync(new MemberRegisteredEvent(memberId));

// 发送到特定端点（跨进程）
await _bus.SendAsync(new ProcessPaymentCommand(orderId), new Uri("rabbitmq://queue/payments"));
```

**消息类型**:
- **Command**: 有且仅有一个处理器，期望得到响应
- **Event**: 可以有多个订阅者，通常无响应
- **Query**: 专门用于查询，返回数据

### 2.3 中间件和生命周期

Wolverine 支持在消息处理前后执行中间件：

```csharp
// 全局中间件
public class LoggingMiddleware
{
    public static async Task Handle(
        IMessageContext context,
        ILogger logger,
        Func<Task> next)
    {
        logger.LogInformation("处理消息: {MessageType}", context.Envelope.MessageType);
        
        var sw = Stopwatch.StartNew();
        await next();
        sw.Stop();
        
        logger.LogInformation("消息处理完成，耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}

// 特定于命令的中间件（使用特性）
[WolverineIgnore] // 跳过某些处理器
[Transactional]   // 自动事务管理
public class CreateOrderHandler
{
    public Task Handle(CreateOrderCommand command) { /* ... */ }
}
```

### 2.4 HTTP 集成

Wolverine 可以轻松地将处理器暴露为 HTTP 端点：

```csharp
// 在 Program.cs 中配置
app.MapWolverineEndpoints();

// 使用特性标记端点
public class GetMemberHandler
{
    [WolverineGet("/api/members/{id}")]
    public async Task<Member?> Handle(
        Guid id,
        IDocumentSession session)
    {
        return await session.LoadAsync<Member>(id);
    }
}

// 或使用 MinimalAPI 风格
app.MapPost("/api/members", async (
    CreateMemberCommand command,
    IMessageBus bus) =>
{
    var result = await bus.InvokeAsync<MemberCreated>(command);
    return Results.Created($"/api/members/{result.MemberId}", result);
});
```

### 2.5 后台任务和定时任务

```csharp
// 延迟执行
await _bus.ScheduleAsync(
    new SendWelcomeEmailCommand(memberId),
    TimeSpan.FromMinutes(5)
);

// 定时任务
[WolverineHandler]
public static class DailyReportJob
{
    [Schedule("0 0 * * *")] // 每天午夜执行（Cron 表达式）
    public static async Task Execute(IDocumentSession session)
    {
        // 生成日报
    }
}
```

### 2.6 持久化收件箱/发件箱（Transactional Outbox）

Wolverine 与 Marten 集成，提供了持久化的消息处理：

```csharp
// 配置持久化收件箱/发件箱
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    // 启用 Wolverine 的收件箱/发件箱
    opts.IntegrateWithWolverine();
});

builder.Host.UseWolverine(opts =>
{
    // 使用 Marten 作为持久化层
    opts.PersistMessagesWithMarten();
    
    // 配置本地队列
    opts.LocalQueue("important")
        .UseDurableInbox(); // 持久化收件箱，保证消息不丢失
});
```

**优势**:
- 保证消息至少被处理一次
- 与数据库事务集成，保证一致性
- 自动重试失败的消息

## 3. 在本项目中的应用

### 3.1 项目配置

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 Marten
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Postgres")!);
    opts.IntegrateWithWolverine();
    
    // 注册实体
    opts.Schema.For<Table>();
    opts.Schema.For<TableSession>();
    opts.Schema.For<Member>();
});

// 添加 Wolverine
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithMarten();
    
    // 自动发现处理器
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    
    // 配置本地队列
    opts.LocalQueue("billing").UseDurableInbox();
    opts.LocalQueue("notifications").UseDurableInbox();
    
    // 配置外部传输（可选）
    // opts.UseRabbitMq(rabbit =>
    // {
    //     rabbit.HostName = "localhost";
    // });
});

var app = builder.Build();

// 映射 Wolverine 端点
app.MapWolverineEndpoints();

app.Run();
```

### 3.2 典型业务流程

**场景：用户开台**

```csharp
// 1. HTTP 请求到达
app.MapPost("/api/sessions/start", async (
    StartSessionCommand command,
    IMessageBus bus) =>
{
    var result = await bus.InvokeAsync<Result<Guid>>(command);
    return result.IsSuccess 
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
});

// 2. Wolverine 路由到处理器
public class StartSessionHandler
{
    public async Task<Result<Guid>> Handle(
        StartSessionCommand command,
        IDocumentSession session,
        IMessageBus bus)
    {
        // 业务逻辑
        var table = await session.LoadAsync<Table>(command.TableId);
        if (table == null)
            return Result.Fail<Guid>("台球桌不存在");

        var tableSession = TableSession.Start(
            command.TableId,
            command.MemberId,
            DateTime.UtcNow
        );

        session.Store(tableSession);
        await session.SaveChangesAsync();

        // 发布领域事件（异步处理）
        await bus.PublishAsync(new SessionStartedEvent(tableSession.Id));

        return Result.Ok(tableSession.Id);
    }
}

// 3. 事件处理器响应
public class SessionStartedEventHandler
{
    // 记录审计日志
    public Task Handle(SessionStartedEvent evt, IDocumentSession session)
    {
        var auditLog = new AuditLog
        {
            Action = "SessionStarted",
            EntityId = evt.SessionId,
            Timestamp = DateTime.UtcNow
        };
        session.Store(auditLog);
        return session.SaveChangesAsync();
    }
}

// 4. 另一个事件处理器（并行执行）
public class NotificationHandler
{
    public async Task Handle(SessionStartedEvent evt, IEmailService email)
    {
        // 发送通知（如果是会员）
        // await email.SendAsync(...);
    }
}
```

### 3.3 错误处理和重试

```csharp
// 在 Program.cs 中配置
builder.Host.UseWolverine(opts =>
{
    // 全局重试策略
    opts.Policies.OnException<HttpRequestException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
    // 特定消息的策略
    opts.Policies.ForMessagesOfType<ProcessPaymentCommand>()
        .MaximumAttempts(5)
        .OnException<PaymentGatewayException>()
        .RetryWithCooldown(1.Seconds(), 5.Seconds(), 10.Seconds());
});

// 在处理器中处理失败
public class ProcessPaymentHandler
{
    public async Task<PaymentResult> Handle(
        ProcessPaymentCommand command,
        IPaymentGateway gateway,
        ILogger logger)
    {
        try
        {
            var result = await gateway.ChargeAsync(command.Amount);
            return PaymentResult.Success(result.TransactionId);
        }
        catch (PaymentGatewayException ex)
        {
            logger.LogError(ex, "支付失败: {OrderId}", command.OrderId);
            
            // Wolverine 会根据配置的策略自动重试
            throw;
        }
    }
}
```

## 4. 测试支持

Wolverine 提供了优秀的测试支持：

```csharp
public class StartSessionHandlerTests
{
    [Fact]
    public async Task Should_Start_Session_Successfully()
    {
        // 使用 Alba 和 Wolverine 的测试工具
        await using var host = await AlbaHost.For<Program>(builder =>
        {
            // 配置测试环境
            builder.ConfigureServices(services =>
            {
                services.AddMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.DatabaseSchemaName = "test";
                });
            });
        });

        // 发送命令
        var result = await host.InvokeMessageAndWaitAsync<Result<Guid>>(
            new StartSessionCommand(tableId, memberId)
        );

        // 断言
        result.IsSuccess.ShouldBeTrue();
        
        // 验证数据
        var session = host.Services.GetRequiredService<IDocumentSession>();
        var tableSession = await session.LoadAsync<TableSession>(result.Value);
        tableSession.ShouldNotBeNull();
    }
}
```

## 5. 与 MediatR 的对比

| 特性 | Wolverine | MediatR |
|------|-----------|---------|
| 配置方式 | 约定优于配置 | 显式注册 |
| 依赖注入 | 方法参数自动注入 | 构造函数注入 |
| 中间件 | 内置，基于约定 | 需要 Pipeline Behaviors |
| 消息传输 | 支持进程内和跨进程 | 仅进程内 |
| 持久化 | 与 Marten 集成 | 需要自行实现 |
| 学习曲线 | 中等（约定较多） | 较低（模式简单） |
| 性能 | 高（编译时代码生成） | 中等（反射） |

**选择建议**:
- 如果需要消息持久化、跨进程通信、后台任务 → 选择 Wolverine
- 如果只需要简单的 CQRS 模式，团队熟悉 MediatR → 选择 MediatR

## 6. 最佳实践

### 6.1 命名约定

```csharp
// 命令: 动词 + 名词 + Command
CreateMemberCommand
UpdateTableStatusCommand
ProcessPaymentCommand

// 事件: 名词 + 动词过去式 + Event
MemberCreatedEvent
SessionStartedEvent
PaymentProcessedEvent

// 查询: Get/Find/Search + 名词 + Query
GetMemberByIdQuery
SearchTablesQuery
FindActiveSessionsQuery

// 处理器: 消息名 + Handler
CreateMemberHandler
SessionStartedEventHandler
GetMemberByIdHandler
```

### 6.2 处理器组织

```
Features/
  Members/
    CreateMember/
      CreateMemberCommand.cs
      CreateMemberHandler.cs
      CreateMemberValidator.cs
      MemberCreatedEvent.cs
    UpdateMember/
      ...
    GetMember/
      GetMemberByIdQuery.cs
      GetMemberByIdHandler.cs
```

### 6.3 避免的陷阱

❌ **不要在处理器中直接调用其他处理器**:
```csharp
// 错误
public class CreateOrderHandler
{
    private readonly CreatePaymentHandler _paymentHandler;
    
    public Task Handle(CreateOrderCommand cmd)
    {
        // 不要这样做
        await _paymentHandler.Handle(new CreatePaymentCommand());
    }
}
```

✅ **使用消息总线**:
```csharp
// 正确
public class CreateOrderHandler
{
    public async Task Handle(CreateOrderCommand cmd, IMessageBus bus)
    {
        // 使用消息总线
        await bus.InvokeAsync(new CreatePaymentCommand());
    }
}
```

❌ **不要在处理器中包含太多逻辑**:
```csharp
// 处理器应该是薄的编排层
public class CreateOrderHandler
{
    public async Task Handle(CreateOrderCommand cmd, IOrderService orderService)
    {
        // 将复杂逻辑委托给领域服务
        var order = await orderService.CreateAsync(cmd);
        return order;
    }
}
```

## 7. 监控和诊断

Wolverine 提供了丰富的诊断信息：

```csharp
// 启用诊断日志
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);

// OpenTelemetry 集成
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddWolverineInstrumentation();
    });

// 健康检查
builder.Services.AddHealthChecks()
    .AddWolverine();
```

**查看消息状态**:
```sql
-- Wolverine 使用 Marten 存储消息
SELECT * FROM wolverine_incoming_messages;
SELECT * FROM wolverine_outgoing_messages;
SELECT * FROM wolverine_dead_letters; -- 失败的消息
```

## 8. 参考资源

- [Wolverine 官方文档](https://wolverine.netlify.app/)
- [Wolverine GitHub](https://github.com/JasperFx/wolverine)
- [示例项目](https://github.com/JasperFx/wolverine/tree/main/samples)
- [与 MediatR 的对比](https://jeremydmiller.com/2023/01/09/wolverine-versus-mediatr/)

---

**最后更新**: 2024-01-15  
**负责人**: 架构团队  
**审核状态**: 待审核
