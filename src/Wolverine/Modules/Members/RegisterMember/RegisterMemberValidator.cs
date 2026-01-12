using FluentValidation;

namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员命令验证器
/// Register member command validator
/// </summary>
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("姓名不能为空")
            .MaximumLength(50).WithMessage("姓名不能超过50个字符");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少6个字符");
    }
}
