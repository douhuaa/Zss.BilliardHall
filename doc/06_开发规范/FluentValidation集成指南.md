# FluentValidation 集成指南

> **目标读者**: 所有使用 Wolverine 开发 Command/Query 的开发人员
>
> **阅读时间**: 15 分钟
>
> **前置要求**: 
> - 熟悉 Wolverine Handler 基础
> - 了解 FluentValidation 库
> - 阅读过 [Wolverine快速上手指南](../03_系统架构设计/Wolverine快速上手指南.md)

---

## 一、为什么需要 FluentValidation

### 1.1 验证层级划分

在 Wolverine 垂直切片架构中，验证分为两个层次：

**输入验证（FluentValidation）**：
- 验证数据格式、类型、长度
- 验证必填字段
- 验证数据范围
- **在 Handler 执行前自动拦截**

**业务规则验证（Handler 中）**：
- 验证业务状态（如库存是否充足）
- 验证业务规则（如会员等级是否满足条件）
- 验证数据一致性（如订单状态是否允许操作）
- **在 Handler 中使用 Result 模式返回**

### 1.2 核心原则

**✅ FluentValidation 处理**：简单、无状态的输入校验
**✅ Handler 处理**：复杂、需要数据库查询的业务规则
**❌ 避免**：在 FluentValidation 中执行重量级操作（如外部 API 调用）

---

## 二、安装与配置

### 2.1 安装依赖

```bash
dotnet add package Wolverine.Http.FluentValidation
```

> **注意**: `Wolverine.Http.FluentValidation` 包已包含 `FluentValidation` 依赖，无需单独安装。

### 2.2 全局配置

在 `Program.cs` 中启用 FluentValidation：

```csharp
using Wolverine;
using Wolverine.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// 配置 Wolverine
builder.Host.UseWolverine(opts =>
{
    // 启用 FluentValidation 自动验证
    opts.UseFluentValidation();
    
    // 自动应用事务（可选）
    opts.Policies.AutoApplyTransactions();
    
    // 自动发现所有 Validator
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
});

var app = builder.Build();
app.MapWolverineEndpoints();
app.Run();
```

**配置说明**：
- `UseFluentValidation()`：启用自动验证，Wolverine 会在 Handler 执行前自动调用 Validator
- `Discovery.IncludeAssembly()`：自动扫描并注册所有 Validator 类

---

## 三、基础验证示例

### 3.1 简单验证器

```csharp
// 位置：Modules/Members/RegisterMember/RegisterMemberValidator.cs
namespace Zss.BilliardHall.Modules.Members.RegisterMember;

using FluentValidation;

/// <summary>
/// 注册会员命令验证器
/// </summary>
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        // 姓名验证
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("会员姓名不能为空")
            .MaximumLength(50).WithMessage("会员姓名不能超过50个字符")
            .Matches(@"^[\u4e00-\u9fa5a-zA-Z\s]+$").WithMessage("会员姓名只能包含中文、英文和空格");
        
        // 手机号验证
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");
        
        // 邮箱验证（可选字段）
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        // 初始余额验证
        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("初始余额不能为负数")
            .LessThanOrEqualTo(100000).WithMessage("初始余额不能超过100000元");
    }
}
```

### 3.2 Command 定义

```csharp
// 位置：Modules/Members/RegisterMember/RegisterMember.cs
namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员命令
/// </summary>
public sealed record RegisterMember(
    string Name,
    string Phone,
    string? Email,
    decimal InitialBalance
);
```

### 3.3 验证失败响应

当验证失败时，Wolverine 会自动返回 `400 Bad Request`，包含结构化错误信息：

```json
{
  "errors": {
    "Name": ["会员姓名不能为空"],
    "Phone": ["手机号格式不正确"],
    "InitialBalance": ["初始余额不能为负数"]
  }
}
```

---

## 四、高级验证场景

### 4.1 异步验证（数据库查询）

```csharp
// 位置：Modules/Members/UpdateMemberPhone/UpdateMemberPhoneValidator.cs
namespace Zss.BilliardHall.Modules.Members.UpdateMemberPhone;

using FluentValidation;
using Marten;

/// <summary>
/// 更新会员手机号验证器
/// 包含异步数据库查询验证
/// </summary>
public sealed class UpdateMemberPhoneValidator : AbstractValidator<UpdateMemberPhone>
{
    public UpdateMemberPhoneValidator(IDocumentSession session)
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确")
            .MustAsync(async (phone, ct) =>
            {
                // 异步验证：检查手机号是否已被其他会员使用
                var exists = await session.Query<Member>()
                    .AnyAsync(m => m.Phone == phone, ct);
                return !exists;
            })
            .WithMessage("该手机号已被注册");
    }
}
```

**注意事项**：
- 异步验证会增加数据库查询开销
- 只在必要时使用（如唯一性验证）
- 考虑使用缓存优化性能

### 4.2 条件验证

```csharp
// 位置：Modules/Sessions/StartSession/StartSessionValidator.cs
namespace Zss.BilliardHall.Modules.Sessions.StartSession;

using FluentValidation;

/// <summary>
/// 开始时段验证器
/// 展示条件验证逻辑
/// </summary>
public sealed class StartSessionValidator : AbstractValidator<StartSession>
{
    public StartSessionValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty().WithMessage("桌台ID不能为空");
        
        // 条件验证：散客必须提供姓名
        When(x => x.MemberId == null, () =>
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("散客必须提供姓名")
                .MaximumLength(50).WithMessage("姓名不能超过50个字符");
        });
        
        // 条件验证：会员不应提供散客姓名
        When(x => x.MemberId != null, () =>
        {
            RuleFor(x => x.CustomerName)
                .Empty().WithMessage("会员不应提供散客姓名");
        });
    }
}
```

### 4.3 自定义验证规则

```csharp
// 位置：BuildingBlocks/Validation/CustomValidators.cs
namespace Zss.BilliardHall.BuildingBlocks.Validation;

using FluentValidation;

/// <summary>
/// 自定义验证扩展方法
/// </summary>
public static class CustomValidators
{
    /// <summary>
    /// 验证中国身份证号码
    /// </summary>
    public static IRuleBuilderOptions<T, string> ChineseIdCard<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\d{17}[\dXx]$")
            .WithMessage("身份证号码格式不正确")
            .Must(BeValidIdCard)
            .WithMessage("身份证号码校验位不正确");
    }
    
    private static bool BeValidIdCard(string idCard)
    {
        // 实现身份证校验位算法
        // 详细实现略...
        return true;
    }
    
    /// <summary>
    /// 验证金额精度（最多两位小数）
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> MoneyPrecision<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("金额最多保留两位小数");
    }
}

// 使用自定义验证器
public sealed class CreateMemberValidator : AbstractValidator<CreateMember>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.IdCard)
            .ChineseIdCard();  // 使用自定义验证
        
        RuleFor(x => x.InitialBalance)
            .MoneyPrecision();  // 使用自定义验证
    }
}
```

### 4.4 复杂对象验证

```csharp
// 嵌套对象验证
public sealed record CreateOrder(
    Guid MemberId,
    List<OrderItem> Items
);

public sealed record OrderItem(
    Guid ProductId,
    int Quantity,
    decimal Price
);

public sealed class CreateOrderValidator : AbstractValidator<CreateOrder>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("会员ID不能为空");
        
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("订单项不能为空")
            .Must(items => items.Count <= 50).WithMessage("订单项不能超过50个");
        
        // 验证集合中的每个元素
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}

public sealed class OrderItemValidator : AbstractValidator<OrderItem>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("商品ID不能为空");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0")
            .LessThanOrEqualTo(9999).WithMessage("数量不能超过9999");
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("价格不能为负数");
    }
}
```

---

## 五、命名与组织约定

### 5.1 文件命名

**规范**：
- 文件名：`{Command/Query}Validator.cs`
- 类名：`{Command/Query}Validator`
- 位置：与 Command/Query/Handler 在同一 UseCase 文件夹

**示例**：
```
Modules/Members/
├── RegisterMember/
│   ├── RegisterMember.cs           # Command
│   ├── RegisterMemberHandler.cs    # Handler
│   ├── RegisterMemberValidator.cs  # Validator ✅
│   └── RegisterMemberEndpoint.cs   # Endpoint
```

### 5.2 Validator 类定义

```csharp
// ✅ 推荐：sealed class，继承 AbstractValidator<TCommand>
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    // 构造函数中定义所有验证规则
    public RegisterMemberValidator()
    {
        // ...
    }
}

// ❌ 避免：非 sealed 类
public class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    // ...
}
```

---

## 六、最佳实践

### 6.1 验证职责划分

**✅ FluentValidation 验证**：
- 非空验证（`NotEmpty()`, `NotNull()`）
- 格式验证（`Matches()`, `EmailAddress()`）
- 长度验证（`MaximumLength()`, `MinimumLength()`）
- 范围验证（`GreaterThan()`, `LessThan()`）
- 简单的数据库查询（如唯一性验证）

**✅ Handler 业务规则验证**：
- 状态机验证（如订单状态是否允许取消）
- 库存验证（如商品库存是否充足）
- 权限验证（如会员等级是否满足条件）
- 复杂的跨表查询验证

**示例对比**：
```csharp
// ✅ FluentValidation：简单格式验证
public sealed class CreateOrderValidator : AbstractValidator<CreateOrder>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0");
    }
}

// ✅ Handler：复杂业务规则验证
public sealed class CreateOrderHandler
{
    public async Task<Result<Guid>> Handle(
        CreateOrder command,
        IDocumentSession session)
    {
        // 业务规则验证：检查库存
        var product = await session.LoadAsync<Product>(command.ProductId);
        if (product.Stock < command.Quantity)
            return Result.Fail<Guid>("库存不足");
        
        // ...
    }
}
```

### 6.2 性能考虑

**避免在 Validator 中执行重量级操作**：

```csharp
// ❌ 避免：外部 API 调用
public sealed class CreateOrderValidator : AbstractValidator<CreateOrder>
{
    public CreateOrderValidator(IHttpClientFactory httpClientFactory)
    {
        RuleFor(x => x.CouponCode)
            .MustAsync(async (code, ct) =>
            {
                // ❌ 不要在 Validator 中调用外部 API
                var client = httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://api.example.com/coupons/{code}");
                return response.IsSuccessStatusCode;
            });
    }
}

// ✅ 推荐：在 Handler 中处理
public sealed class CreateOrderHandler
{
    public async Task<Result<Guid>> Handle(
        CreateOrder command,
        IHttpClientFactory httpClientFactory)
    {
        // ✅ 在 Handler 中调用外部 API
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://api.example.com/coupons/{command.CouponCode}");
        if (!response.IsSuccessStatusCode)
            return Result.Fail<Guid>("优惠券无效");
        
        // ...
    }
}
```

### 6.3 错误消息国际化

```csharp
// 使用资源文件
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(50).WithMessage(ValidationMessages.NameTooLong);
    }
}

// ValidationMessages.resx
// NameRequired: "会员姓名不能为空"
// NameTooLong: "会员姓名不能超过50个字符"
```

### 6.4 条件验证优化

使用 `When()` 进行条件验证，避免复杂的 if-else 逻辑：

```csharp
// ✅ 推荐：使用 When()
public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfile>
{
    public UpdateProfileValidator()
    {
        // 仅在修改邮箱时验证邮箱格式
        When(x => !string.IsNullOrEmpty(x.NewEmail), () =>
        {
            RuleFor(x => x.NewEmail)
                .EmailAddress().WithMessage("邮箱格式不正确");
        });
        
        // 仅在修改手机号时验证手机号格式
        When(x => !string.IsNullOrEmpty(x.NewPhone), () =>
        {
            RuleFor(x => x.NewPhone)
                .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");
        });
    }
}

// ❌ 避免：在 Validator 中使用 if-else
public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfile>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.NewEmail)
            .Must((model, email) =>
            {
                if (string.IsNullOrEmpty(email))
                    return true;
                return IsValidEmail(email);
            });
    }
}
```

---

## 七、测试验证器

### 7.1 单元测试

```csharp
using FluentValidation.TestHelper;
using Xunit;

public class RegisterMemberValidatorTests
{
    private readonly RegisterMemberValidator _validator;
    
    public RegisterMemberValidatorTests()
    {
        _validator = new RegisterMemberValidator();
    }
    
    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new RegisterMember("", "13800138000", null, 0);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("会员姓名不能为空");
    }
    
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Valid()
    {
        // Arrange
        var command = new RegisterMember("张三", "13800138000", "zhang@example.com", 100);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

---

## 八、常见问题

### Q1: 验证器未生效，如何排查？

1. 检查是否调用了 `opts.UseFluentValidation()`
2. 检查 Validator 类是否为 `public`
3. 检查 Validator 是否在正确的命名空间
4. 启用 Debug 日志查看 Wolverine 是否发现了 Validator

### Q2: 如何禁用特定 Command 的验证？

```csharp
// 在 Endpoint 中使用 [WolverineIgnore] 特性
[WolverinePost("/api/members")]
[WolverineIgnore(typeof(FluentValidationPolicy))]
public static RegisterMember Post(RegisterMemberRequest request)
    => new(request.Name, request.Phone, request.Email, request.InitialBalance);
```

### Q3: 验证失败后如何自定义响应格式？

```csharp
// 创建自定义 ValidationPolicy
public class CustomValidationPolicy : IWolverinePolicy
{
    public void Apply(IReadOnlyList<IChain> chains, GenerationRules rules, IServiceContainer container)
    {
        // 自定义验证失败响应格式
        // 详细实现略...
    }
}

// 在 Wolverine 配置中注册
opts.Policies.Add<CustomValidationPolicy>();
```

---

## 九、相关文档

- [Wolverine 快速上手指南](../03_系统架构设计/Wolverine快速上手指南.md) - 第 3.1 节：带验证的 Command
- [切片约束](切片约束.md) - 第 6 节：处理器约束
- [打球时段模块](../04_模块设计/打球时段模块.md) - StartSessionValidator 完整示例
- [FluentValidation 官方文档](https://docs.fluentvalidation.net/)

---

## 十、TODO 标签使用

在代码中添加 TODO 标签时，引用本文档：

```csharp
// TODO(validation): 添加 Wolverine.Http.FluentValidation 验证器
// 详细文档：doc/06_开发规范/FluentValidation集成指南.md
// 快速上手：doc/03_系统架构设计/Wolverine快速上手指南.md #section-validation
```

---

**最后更新**: 2026-01-11  
**负责人**: 架构团队  
**审核状态**: ✅ 已审核
