# Wolverine 端点约定

> **版本**: 1.0.0  
> **更新日期**: 2026-01-11  
> **适用范围**: 所有使用 Wolverine 框架的项目

---

## 1. 概述

本文档定义 Wolverine 框架中命令（Command）、查询（Query）、事件（Event）的命名约定和 Handler 的组织规则，确保团队成员遵循统一的编码规范。

**核心原则**:
- 清晰的命名规范，提高代码可读性
- 垂直切片架构，按业务能力组织代码
- Handler 即 Application Service，无需额外的服务层

---

## 2. 命名约定

### 2.1 命令（Command）

**定义**: 表示系统中的写操作（创建、更新、删除）。

**命名规则**: `动词 + 名词` 或 `动词 + 名词 + Command`（推荐加 `Command` 后缀）

**示例**:
```csharp
// ✅ 推荐（带 Command 后缀）
public sealed record CreateMember(
    string Name,
    string Phone,
    string Email
);

public sealed record CreateMemberCommand(
    string Name,
    string Phone,
    string Email
);

public sealed record UpdateMemberProfile(
    Guid MemberId,
    string Name,
    string Email
);

public sealed record StartSession(
    Guid TableId,
    Guid MemberId
);

public sealed record ProcessPayment(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod
);

// ❌ 避免（命名不清晰）
public sealed record Member(...);           // 太宽泛
public sealed record MemberInfo(...);       // 不是操作
public sealed record DoSomething(...);      // 模糊不清
```

**注意**:
- 使用 `record` 类型（不可变数据结构）
- 使用 `sealed` 防止继承
- 优先使用主构造函数（Primary Constructor）

---

### 2.2 查询（Query）

**定义**: 表示系统中的读操作（查询、搜索、获取）。

**命名规则**: `Get/Find/Search + 名词` 或 `Get/Find/Search + 名词 + Query`（推荐加 `Query` 后缀）

**示例**:
```csharp
// ✅ 推荐（带 Query 后缀）
public sealed record GetMemberById(Guid Id);

public sealed record GetMemberByIdQuery(Guid Id);

public sealed record SearchMembers(
    string? Keyword,
    int Page = 1,
    int PageSize = 20
);

public sealed record FindActiveSessionsQuery(
    DateTime? StartDate,
    DateTime? EndDate
);

public sealed record ListAvailableTables(
    TableType? Type
);

// ❌ 避免
public sealed record Member(Guid Id);        // 不清楚是命令还是查询
public sealed record GetData(...);           // 太宽泛
public sealed record FetchInfo(...);         // 使用 Get/Find/Search
```

**注意**:
- 查询不应修改系统状态（符合 CQS 原则）
- 查询可以返回 DTO 或领域实体

---

### 2.3 事件（Event）

**定义**: 表示系统中已经发生的事情（过去式）。

**命名规则**: `名词 + 动词过去式` 或 `名词 + 动词过去式 + Event`（推荐加 `Event` 后缀）

**示例**:
```csharp
// ✅ 推荐（带 Event 后缀）
public sealed record MemberCreated(
    Guid MemberId,
    string Name,
    DateTime CreatedAt
);

public sealed record MemberCreatedEvent(
    Guid MemberId,
    string Name,
    DateTime CreatedAt
);

public sealed record SessionStarted(
    Guid SessionId,
    Guid TableId,
    Guid MemberId,
    DateTime StartTime
);

public sealed record PaymentCompleted(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime CompletedAt
);

public sealed record TableStatusChanged(
    Guid TableId,
    TableStatus OldStatus,
    TableStatus NewStatus,
    DateTime ChangedAt
);

// ❌ 避免
public sealed record MemberCreate(...);      // 应该用过去式
public sealed record CreateMember(...);      // 这是命令，不是事件
public sealed record MemberEvent(...);       // 不清楚发生了什么
```

**注意**:
- 事件名称使用**过去式**
- 事件应该是不可变的（使用 `record`）
- 事件可以携带必要的上下文信息

---

### 2.4 处理器（Handler）

**定义**: 处理命令、查询或事件的业务逻辑单元。

**命名规则**: `切片名 + Handler`

**示例**:
```csharp
// ✅ 推荐
public sealed class CreateMemberHandler
{
    public async Task<Result<Guid>> Handle(
        CreateMember command,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        // 业务逻辑
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

        return Result.Ok(member.Id);
    }
}

public sealed class GetMemberByIdHandler
{
    public async Task<Member?> Handle(
        GetMemberById query,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        return await session.LoadAsync<Member>(query.Id, ct);
    }
}

// ❌ 避免
public class MemberService { }               // 不是 Handler
public class MemberApplicationService { }    // 太复杂
public class Handler { }                     // 太宽泛
```

**注意**:
- Handler 方法统一命名为 `Handle`（Wolverine 约定）
- Handler 参数顺序：命令/查询、依赖服务、CancellationToken
- Handler 应该是 `sealed class`

---

### 2.5 端点（Endpoint）

**定义**: HTTP 端点，将 HTTP 请求映射到命令或查询。

**命名规则**: `切片名 + Endpoint`

**示例**:
```csharp
// ✅ 推荐
public sealed class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public static CreateMember Post(CreateMemberRequest request)
        => new(request.Name, request.Phone, request.Email);

    public sealed record CreateMemberRequest(
        string Name,
        string Phone,
        string Email
    );
}

public sealed class GetMemberByIdEndpoint
{
    [WolverineGet("/api/members/{id}")]
    public static GetMemberById Get(Guid id)
        => new(id);
}

// ❌ 避免
public class MemberController { }            // 不使用 Controller
public class MemberApi { }                   // 命名不清晰
```

**注意**:
- 使用 Wolverine 特性：`[WolverineGet]`、`[WolverinePost]`、`[WolverinePut]`、`[WolverineDelete]`
- 端点只负责映射，不包含业务逻辑
- 端点方法可以是静态方法（性能优化）

---

### 2.6 验证器（Validator）

**定义**: 使用 FluentValidation 进行输入验证。

**命名规则**: `切片名 + Validator`

**示例**:
```csharp
// ✅ 推荐
public sealed class CreateMemberValidator : AbstractValidator<CreateMember>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("姓名不能为空")
            .MaximumLength(50).WithMessage("姓名长度不能超过 50 个字符");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

// ❌ 避免
public class MemberValidation { }            // 使用 Validator 后缀
public class CreateMemberRules { }           // 使用 Validator 后缀
```

**注意**:
- Validator 应该只包含简单的验证规则（格式、长度、非空等）
- 复杂的业务规则应该在 Handler 中处理

---

## 3. Handler 放置规则

### 3.1 垂直切片架构

**原则**: 一个 Use Case = 一个文件夹

**标准结构**:
```
Modules/
  {ModuleName}/                    # 模块名称（复数）
    {FeatureName}/                 # 功能切片（动词+名词）
      {Feature}.cs                 # Command/Query 定义
      {Feature}Handler.cs          # Handler 实现
      {Feature}Endpoint.cs         # HTTP 端点
      {Feature}Validator.cs        # 验证器（可选）
      {Event}Event.cs              # 事件（可选）
```

**示例**:
```
Modules/
  Members/                         # 会员模块
    CreateMember/
      CreateMember.cs              # Command 定义
      CreateMemberHandler.cs       # Handler 实现
      CreateMemberEndpoint.cs      # HTTP 端点
      CreateMemberValidator.cs     # 输入验证
      MemberCreated.cs             # 领域事件
    UpdateMemberProfile/
      UpdateMemberProfile.cs
      UpdateMemberProfileHandler.cs
      UpdateMemberProfileEndpoint.cs
    GetMemberById/
      GetMemberById.cs
      GetMemberByIdHandler.cs
      GetMemberByIdEndpoint.cs
    Member.cs                      # 聚合根
    MemberTier.cs                  # 枚举/值对象
  
  Sessions/                        # 打球时段模块
    StartSession/
      StartSession.cs
      StartSessionHandler.cs
      StartSessionEndpoint.cs
      SessionStarted.cs
    EndSession/
      EndSession.cs
      EndSessionHandler.cs
      EndSessionEndpoint.cs
      SessionEnded.cs
    Session.cs                     # 聚合根
```

---

### 3.2 文件命名规则

| 文件类型 | 命名规则 | 示例 |
|---------|---------|------|
| Command/Query | `{Feature}.cs` | `CreateMember.cs` |
| Handler | `{Feature}Handler.cs` | `CreateMemberHandler.cs` |
| Endpoint | `{Feature}Endpoint.cs` | `CreateMemberEndpoint.cs` |
| Validator | `{Feature}Validator.cs` | `CreateMemberValidator.cs` |
| Event | `{Event}.cs` 或 `{Event}Event.cs` | `MemberCreated.cs` |
| 聚合根 | `{Entity}.cs` | `Member.cs` |
| 值对象/枚举 | `{Name}.cs` | `MemberTier.cs` |

---

### 3.3 命名空间约定

**规则**: 命名空间 = 模块路径

**示例**:
```csharp
// 文件路径：Modules/Members/CreateMember/CreateMember.cs
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed record CreateMember(
    string Name,
    string Phone,
    string Email
);

// 文件路径：Modules/Sessions/StartSession/StartSessionHandler.cs
namespace Zss.BilliardHall.Modules.Sessions.StartSession;

public sealed class StartSessionHandler
{
    // ...
}
```

**注意**:
- 使用 C# 10+ 的 file-scoped namespace（文件范围命名空间）
- 命名空间与文件夹路径保持一致

---

## 4. Handler 实现约定

### 4.1 Handler 方法签名

**标准签名**:
```csharp
public async Task<TResult> Handle(
    TCommand command,          // 命令或查询
    IDocumentSession session,  // 数据访问
    CancellationToken ct = default)
{
    // 业务逻辑
}
```

**参数顺序**:
1. 命令/查询（必需）
2. 依赖服务（可选，按需注入）
3. `CancellationToken`（可选，默认值 `default`）

**示例**:
```csharp
// 简单 Handler（只需数据访问）
public sealed class CreateMemberHandler
{
    public async Task<Guid> Handle(
        CreateMember command,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Phone = command.Phone,
            Email = command.Email
        };

        session.Store(member);
        await session.SaveChangesAsync(ct);

        return member.Id;
    }
}

// 复杂 Handler（需要多个依赖）
public sealed class ProcessPaymentHandler
{
    public async Task<Result> Handle(
        ProcessPayment command,
        IDocumentSession session,
        IPaymentGateway paymentGateway,
        IMessageBus messageBus,
        ILogger<ProcessPaymentHandler> logger,
        CancellationToken ct = default)
    {
        // 业务逻辑
        logger.LogInformation("处理支付 {PaymentId}", command.PaymentId);

        var result = await paymentGateway.ChargeAsync(command.Amount, ct);
        if (!result.IsSuccess)
        {
            return Result.Fail("支付失败");
        }

        // 发布事件
        await messageBus.PublishAsync(new PaymentCompleted(
            command.PaymentId,
            command.OrderId,
            command.Amount,
            DateTime.UtcNow
        ), ct);

        return Result.Ok();
    }
}
```

---

### 4.2 事务管理

**推荐**: 使用 Wolverine 的 `[Transactional]` 特性自动管理事务。

**示例**:
```csharp
public sealed class CreateMemberHandler
{
    [Transactional]  // 自动开启事务 + Outbox 模式
    public async Task<Guid> Handle(
        CreateMember command,
        IDocumentSession session,
        IMessageBus messageBus,
        CancellationToken ct = default)
    {
        var member = new Member { /* ... */ };
        session.Store(member);

        // 事件会自动存入 Outbox，确保事务一致性
        await messageBus.PublishAsync(new MemberCreated(member.Id, member.Name, DateTime.UtcNow));

        // 无需手动 SaveChangesAsync，Wolverine 会自动处理
        return member.Id;
    }
}
```

**注意**:
- 使用 `[Transactional]` 时，无需手动调用 `SaveChangesAsync`
- 事件会自动存入 Outbox，确保最终一致性

---

### 4.3 错误处理

**推荐**: 使用 `Result` 模式返回结果（成功或失败）。

**示例**:
```csharp
public sealed class UpdateMemberProfileHandler
{
    public async Task<Result> Handle(
        UpdateMemberProfile command,
        IDocumentSession session,
        CancellationToken ct = default)
    {
        var member = await session.LoadAsync<Member>(command.MemberId, ct);
        if (member is null)
        {
            return Result.Fail($"会员不存在：{command.MemberId}");
        }

        member.Name = command.Name;
        member.Email = command.Email;
        member.UpdatedAt = DateTime.UtcNow;

        session.Update(member);
        await session.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
```

**注意**:
- 业务错误使用 `Result.Fail`
- 系统异常直接抛出（如 `ArgumentNullException`、`InvalidOperationException`）

---

## 5. 跨模块通信

### 5.1 同步调用

**场景**: 需要立即获取结果的跨模块调用。

**使用**: `IMessageBus.InvokeAsync()`

**示例**:
```csharp
public sealed class EndSessionHandler
{
    public async Task<Result> Handle(
        EndSession command,
        IMessageBus messageBus,
        CancellationToken ct = default)
    {
        // 同步调用计费模块
        var billingResult = await messageBus.InvokeAsync<CalculateBillingResult>(
            new CalculateBilling(command.SessionId),
            ct
        );

        if (!billingResult.IsSuccess)
        {
            return Result.Fail("计费失败");
        }

        // 继续处理...
        return Result.Ok();
    }
}
```

---

### 5.2 异步通信

**场景**: 不需要立即响应的跨模块通知。

**使用**: `IMessageBus.PublishAsync()`

**示例**:
```csharp
public sealed class CompletePaymentHandler
{
    [Transactional]
    public async Task Handle(
        CompletePayment command,
        IDocumentSession session,
        IMessageBus messageBus,
        CancellationToken ct = default)
    {
        // 更新支付状态
        var payment = await session.LoadAsync<Payment>(command.PaymentId, ct);
        payment.Status = PaymentStatus.Completed;
        session.Update(payment);

        // 异步发布事件（其他模块订阅）
        await messageBus.PublishAsync(new PaymentCompleted(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            DateTime.UtcNow
        ));

        // Wolverine 会自动保存事件到 Outbox
    }
}
```

**注意**:
- 使用 `[Transactional]` 确保事件和数据修改在同一事务中
- 事件会自动存入 Outbox，确保最终一致性

---

## 6. 配置与启动

### 6.1 统一配置

在 `Program.cs` 中使用 `AddWolverineDefaults()` 启用 Wolverine：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 ServiceDefaults
builder.AddServiceDefaults();

// 添加 Wolverine 默认配置
builder.AddWolverineDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();
```

---

### 6.2 模块扫描

Wolverine 会自动扫描程序集中的 Handler、Endpoint 和 Validator：

```csharp
// 在 AddWolverineDefaults() 中已配置
builder.Services.AddWolverine(opts =>
{
    // 自动发现 Handler
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);

    // 集成 FluentValidation
    opts.UseFluentValidation();
});
```

**注意**:
- 确保 Handler、Endpoint 和 Validator 的访问修饰符为 `public` 或 `internal`
- 使用 `sealed class` 防止继承

---

## 7. 最佳实践

### 7.1 命名清晰

✅ **推荐**:
```csharp
public sealed record CreateMember(...);
public sealed record GetMemberById(...);
public sealed record MemberCreated(...);
```

❌ **避免**:
```csharp
public sealed record Member(...);        // 不清楚是命令还是查询
public sealed record DoSomething(...);   // 太模糊
public sealed record Process(...);       // 不知道处理什么
```

---

### 7.2 单一职责

✅ **推荐**:
```csharp
// 一个 Handler 只处理一个命令
public sealed class CreateMemberHandler
{
    public async Task<Guid> Handle(CreateMember command, ...) { }
}

public sealed class UpdateMemberHandler
{
    public async Task Handle(UpdateMember command, ...) { }
}
```

❌ **避免**:
```csharp
// 不要在一个 Handler 中处理多个命令
public sealed class MemberHandler
{
    public async Task<Guid> Handle(CreateMember command, ...) { }
    public async Task Handle(UpdateMember command, ...) { }
    public async Task Handle(DeleteMember command, ...) { }
}
```

---

### 7.3 使用 Result 模式

✅ **推荐**:
```csharp
public sealed class UpdateMemberHandler
{
    public async Task<Result<Member>> Handle(UpdateMember command, ...)
    {
        var member = await session.LoadAsync<Member>(command.Id);
        if (member is null)
        {
            return Result.Fail<Member>("会员不存在");
        }

        // 更新逻辑...
        return Result.Ok(member);
    }
}
```

❌ **避免**:
```csharp
// 不要直接抛出业务异常
public sealed class UpdateMemberHandler
{
    public async Task<Member> Handle(UpdateMember command, ...)
    {
        var member = await session.LoadAsync<Member>(command.Id);
        if (member is null)
        {
            throw new NotFoundException("会员不存在");  // ❌
        }

        return member;
    }
}
```

---

### 7.4 使用 UTC 时间

✅ **推荐**:
```csharp
public sealed class CreateMemberHandler
{
    public async Task<Guid> Handle(CreateMember command, ...)
    {
        var member = new Member
        {
            CreatedAt = DateTime.UtcNow,  // ✅ 使用 UTC
            // ...
        };

        return member.Id;
    }
}
```

❌ **避免**:
```csharp
var member = new Member
{
    CreatedAt = DateTime.Now,  // ❌ 不要使用本地时间
    // ...
};
```

---

## 8. 示例代码

完整的垂直切片示例（会员注册功能）：

**文件结构**:
```
Modules/Members/CreateMember/
├── CreateMember.cs
├── CreateMemberHandler.cs
├── CreateMemberEndpoint.cs
├── CreateMemberValidator.cs
└── MemberCreated.cs
```

**CreateMember.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed record CreateMember(
    string Name,
    string Phone,
    string Email
);
```

**CreateMemberHandler.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed class CreateMemberHandler
{
    [Transactional]
    public async Task<Result<Guid>> Handle(
        CreateMember command,
        IDocumentSession session,
        IMessageBus messageBus,
        CancellationToken ct = default)
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

        await messageBus.PublishAsync(new MemberCreated(
            member.Id,
            member.Name,
            member.CreatedAt
        ));

        return Result.Ok(member.Id);
    }
}
```

**CreateMemberEndpoint.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public static CreateMember Post(CreateMemberRequest request)
        => new(request.Name, request.Phone, request.Email);

    public sealed record CreateMemberRequest(
        string Name,
        string Phone,
        string Email
    );
}
```

**CreateMemberValidator.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed class CreateMemberValidator : AbstractValidator<CreateMember>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("姓名不能为空")
            .MaximumLength(50).WithMessage("姓名长度不能超过 50 个字符");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

**MemberCreated.cs**:
```csharp
namespace Zss.BilliardHall.Modules.Members.CreateMember;

public sealed record MemberCreated(
    Guid MemberId,
    string Name,
    DateTime CreatedAt
);
```

---

## 9. 相关文档

- [Wolverine 模块化架构蓝图](../03_系统架构设计/Wolverine模块化架构蓝图.md)
- [Wolverine 快速上手指南](../03_系统架构设计/Wolverine快速上手指南.md)
- [切片约束](./切片约束.md)
- [FluentValidation 集成指南](./FluentValidation集成指南.md)
- [Saga 使用指南](./Saga使用指南.md)

---

## 10. 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| 1.0.0 | 2026-01-11 | 初始版本，定义命令/查询/事件命名约定和 Handler 组织规则 |

---

**维护者**: 架构团队  
**审核者**: 技术负责人
