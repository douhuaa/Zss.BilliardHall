using FluentValidation;

namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令验证器
/// 职责：确保命令数据符合业务规则
///
/// 自动验证工作流程：
/// 1. Wolverine.AddWolverine() 中配置 UseFluentValidation()
/// 2. 请求到达端点时，Wolverine 自动发现此验证器
/// 3. 验证失败时抛出 FluentValidation.ValidationException
/// 4. GlobalExceptionMiddleware 捕获异常
/// 5. FluentValidationExceptionTranslator 将其转换为 Platform.Exceptions.ValidationException
/// 6. ExceptionProblemDetailsMapper 将异常映射为 400 BadRequest + ValidationProblemDetails
/// </summary>
public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("会员名称不能为空")
            .MinimumLength(2).WithMessage("会员名称至少 2 个字符")
            .MaximumLength(100).WithMessage("会员名称最多 100 个字符");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

