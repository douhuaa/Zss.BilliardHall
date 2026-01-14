# BuildingBlocks - 领域异常处理架构

> 统一的跨模块异常处理基础设施

## 目录结构

```
BuildingBlocks/
├── Core/                           # 核心抽象
│   ├── ErrorDescriptor.cs          # 结构化错误描述符
│   ├── ErrorCodeBuilder.cs         # 流式错误构建器
│   ├── ModuleDomainException.cs    # 模块领域异常基类
│   ├── DomainResult.cs             # 领域操作结果
│   └── ErrorCode.cs                # 旧版错误码（已废弃）
├── Behaviors/                      # 行为/中间件
│   └── DomainExceptionHandler.cs   # 统一异常转换器
├── Contracts/                      # 契约
│   └── Result.cs                   # HTTP 响应结果
└── Exceptions/                     # 通用异常（遗留）
    ├── DomainException.cs          # 旧版领域异常
    └── NotFoundException.cs        # 旧版未找到异常
```

## 核心组件

### 1. ErrorDescriptor（错误描述符）

结构化的错误描述，包含完整业务上下文：

```csharp
public sealed record ErrorDescriptor
{
    string Code { get; }                  // 错误码: {Module}:{Category}.{Specific}
    ErrorCategory Category { get; }       // 错误类别
    string Module { get; }                // 所属模块
    string DefaultMessage { get; }        // 默认消息（中文）
    IReadOnlyDictionary<string, object>? Context { get; } // 上下文数据
}
```

**错误码格式**：`{Module}:{Category}.{Specific}`
- `Members:Validation.InvalidTopUpAmount`
- `Tables:Business.TableOccupied`
- `Sessions:NotFound.Session`

### 2. ErrorCategory（错误类别）

```csharp
public enum ErrorCategory
{
    NotFound,        // 资源未找到 → HTTP 404
    Validation,      // 输入验证失败 → HTTP 400
    Business,        // 业务规则违反 → HTTP 422
    Conflict,        // 资源冲突 → HTTP 409
    Forbidden,       // 权限不足 → HTTP 403
    InvalidStatus    // 无效状态 → HTTP 409
}
```

### 3. ErrorCodeBuilder（流式构建器）

简化 ErrorDescriptor 的创建：

```csharp
var error = ErrorCodeBuilder.ForModule("Members")
    .WithCode("Members:Validation.InvalidTopUpAmount")
    .WithCategory(ErrorCategory.Validation)
    .WithMessage("充值金额必须大于0，实际: {Amount:F2}")
    .AddContext("Amount", amount)
    .Build();
```

### 4. ModuleDomainException（模块异常基类）

每个模块定义自己的领域异常：

```csharp
public sealed class MembersDomainException : ModuleDomainException
{
    public MembersDomainException(ErrorDescriptor errorDescriptor)
        : base(errorDescriptor) { }
}
```

### 5. DomainResult（领域结果）

聚合根方法返回 `DomainResult`，不抛出异常：

```csharp
public DomainResult TopUp(decimal amount)
{
    if (amount <= 0)
        return DomainResult.Fail(
            MemberErrorDescriptors.InvalidTopUpAmount(amount));

    Balance += amount;
    return DomainResult.Success();
}
```

### 6. DomainExceptionHandler（统一转换器）

在 Handler 层统一转换 DomainResult → Result：

```csharp
var domainResult = member.TopUp(command.Amount);
if (!domainResult.IsSuccess)
{
    var (result, statusCode) = DomainExceptionHandler.ToResult(domainResult, logger);
    return (result, null);
}
```

**功能**：
- 自动记录结构化日志
- 自动映射 HTTP 状态码
- 统一错误响应格式

---

## 使用流程

### 1. 定义模块错误（在各模块中）

```csharp
// Modules/Members/MemberErrorDescriptors.cs
internal static class MemberErrorDescriptors
{
    private const string ModuleName = "Members";

    public static ErrorDescriptor MemberNotFound(Guid memberId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:NotFound.Member")
            .WithCategory(ErrorCategory.NotFound)
            .WithMessage("会员不存在: {MemberId}")
            .AddContext("MemberId", memberId)
            .Build();

    public static ErrorDescriptor InvalidTopUpAmount(decimal amount) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidTopUpAmount")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("充值金额必须大于0，实际: {Amount:F2}")
            .AddContext("Amount", amount)
            .Build();
}
```

### 2. 在聚合根中使用

```csharp
// Modules/Members/Member.cs
public class Member
{
    public DomainResult TopUp(decimal amount)
    {
        if (amount <= 0)
            return DomainResult.Fail(
                MemberErrorDescriptors.InvalidTopUpAmount(amount));

        Balance += amount;
        return DomainResult.Success();
    }
}
```

### 3. 在 Handler 中处理

```csharp
// Modules/Members/TopUpBalance/TopUpBalanceHandler.cs
public sealed class TopUpBalanceHandler
{
    [Transactional]
    public async Task<(Result Result, Event? Event)> Handle(...)
    {
        // 资源未找到
        var member = await session.LoadAsync<Member>(command.MemberId);
        if (member == null)
        {
            var error = MemberErrorDescriptors.MemberNotFound(command.MemberId);
            return (Result.Fail(error.FormatMessage(), error.Code), null);
        }

        // 业务规则验证
        var domainResult = member.TopUp(command.Amount);
        if (!domainResult.IsSuccess)
        {
            var (result, _) = DomainExceptionHandler.ToResult(domainResult, logger);
            return (result, null);
        }

        // 成功路径
        session.Store(member);
        return (Result.Success(), new BalanceToppedUp(...));
    }
}
```

### 4. 在 Endpoint 中返回（可选）

```csharp
// Modules/Members/TopUpBalance/TopUpBalanceEndpoint.cs
[WolverinePost("/api/members/{memberId:guid}/topup")]
public static async Task<IResult> Post(Guid memberId, Request request, IMessageBus bus)
{
    var result = await bus.InvokeAsync<Result>(new TopUpBalance(memberId, request.Amount));

    return result.IsSuccess
        ? Results.Ok(new { message = "充值成功" })
        : Results.BadRequest(new { error = result.Error, code = result.ErrorCode });
}
```

---

## 架构原则

### 单一转换点

异常只在**应用边界**（Handler）转换一次：

```
┌─────────────┐
│  Aggregate  │ → DomainResult (不抛异常)
└─────────────┘
       ↓
┌─────────────┐
│   Handler   │ → Result (DomainExceptionHandler.ToResult)
└─────────────┘
       ↓
┌─────────────┐
│  Endpoint   │ → IResult (直接返回 Handler 的 Result)
└─────────────┘
```

### 跨模块一致性

所有模块遵循相同的错误码格式：

| 模块 | NotFound | Validation | Business | Conflict |
|------|----------|------------|----------|----------|
| Members | `Members:NotFound.Member` | `Members:Validation.InvalidTopUpAmount` | `Members:Business.InsufficientBalance` | `Members:Conflict.DuplicatePhone` |
| Tables | `Tables:NotFound.Table` | `Tables:Validation.InvalidCapacity` | `Tables:Business.TableOccupied` | `Tables:Conflict.DuplicateNumber` |
| Sessions | `Sessions:NotFound.Session` | `Sessions:Validation.InvalidDuration` | `Sessions:Business.SessionEnded` | `Sessions:Conflict.ActiveSessionExists` |

### 垂直切片隔离

每个模块定义自己的 `{Module}ErrorDescriptors.cs`，不共享错误定义：

```
Modules/
├── Members/
│   ├── MemberErrorDescriptors.cs      # Members 模块专用
│   └── Member.cs
├── Tables/
│   ├── TableErrorDescriptors.cs       # Tables 模块专用
│   └── Table.cs
└── Sessions/
    ├── SessionErrorDescriptors.cs     # Sessions 模块专用
    └── Session.cs
```

---

## 向后兼容

旧版 `ErrorCode` 和 `DomainException` 标记为 `[Obsolete]`，但仍可使用：

```csharp
// 旧版（已废弃）
return DomainResult.Fail(MemberErrorCodes.InvalidTopUpAmount);

// 新版（推荐）
return DomainResult.Fail(MemberErrorDescriptors.InvalidTopUpAmount(amount));
```

迁移策略：
1. 新代码使用 `ErrorDescriptor`
2. 旧代码逐步迁移（不强制）
3. `DomainExceptionHandler` 同时支持新旧格式

---

## 测试示例

```csharp
[Fact]
public void TopUp_WithZeroAmount_ShouldFail()
{
    // Arrange
    var member = CreateMember(balance: 100m);

    // Act
    var result = member.TopUp(0m);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.ErrorDescriptor.Should().NotBeNull();
    result.ErrorDescriptor!.Code.Should().Be("Members:Validation.InvalidTopUpAmount");
    result.ErrorDescriptor.Category.Should().Be(ErrorCategory.Validation);
    result.ErrorDescriptor.FormatMessage().Should().Contain("充值金额必须大于0");
}
```

---

## 参考文档

- [领域异常处理规范](../../docs/06_开发规范/领域异常处理规范.md) - 完整使用指南
- [代码风格规范](../../docs/06_开发规范/代码风格.md) - 异常处理基础规范
- [Wolverine端点约定](../../docs/06_开发规范/Wolverine端点约定.md) - Endpoint 异常处理

---

## 常见问题

### Q: 为什么不在 BuildingBlocks 中定义通用错误？

A: 遵循垂直切片原则，错误应该表达**特定模块的业务语义**。通用错误（如 `NotFound`）太抽象，无法表达"是哪个模块的哪个资源未找到"。

### Q: 什么时候使用 ModuleDomainException，什么时候使用 DomainResult？

A: 在 Wolverine 架构中：
- ✅ **聚合根方法**：返回 `DomainResult`（不抛异常）
- ✅ **Handler**：返回 `Result`（使用 `DomainExceptionHandler.ToResult` 转换）
- ❌ **不要抛出异常**：除非是技术异常（如数据库连接失败）

### Q: 如何处理跨模块错误？

A: 跨模块通信通过**事件**，不直接调用其他模块的 Handler。每个模块只处理自己的错误。

---

## 版本历史

- **v1.0.0** (2026-01-14): 初始版本，定义统一异常处理架构
