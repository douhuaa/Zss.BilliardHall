using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// Members 模块错误描述符
/// Members module error descriptors
/// </summary>
internal static class MemberErrorDescriptors
{
    private const string ModuleName = "Members";

    // NotFound errors
    public static ErrorDescriptor MemberNotFound(Guid memberId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:NotFound.Member")
            .WithCategory(ErrorCategory.NotFound)
            .WithMessage("会员不存在: {MemberId}")
            .AddContext("MemberId", memberId)
            .Build();

    // Validation errors
    public static ErrorDescriptor InvalidTopUpAmount(decimal amount) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidTopUpAmount")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("充值金额必须大于0，实际: {Amount:F2}")
            .AddContext("Amount", amount)
            .Build();

    public static ErrorDescriptor InvalidDeductAmount(decimal amount) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidDeductAmount")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("扣减金额必须大于0，实际: {Amount:F2}")
            .AddContext("Amount", amount)
            .Build();

    public static ErrorDescriptor InvalidAwardPoints(int points) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidAwardPoints")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("赠送积分必须大于0，实际: {Points}")
            .AddContext("Points", points)
            .Build();

    // Business rule errors
    public static ErrorDescriptor InsufficientBalance(decimal required, decimal available) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Business.InsufficientBalance")
            .WithCategory(ErrorCategory.Business)
            .WithMessage("余额不足，需要: {Required:F2}，可用: {Available:F2}")
            .AddContext("Required", required)
            .AddContext("Available", available)
            .Build();

    // Conflict errors
    public static ErrorDescriptor DuplicatePhone(string phone) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Conflict.DuplicatePhone")
            .WithCategory(ErrorCategory.Conflict)
            .WithMessage("手机号已被注册: {Phone}")
            .AddContext("Phone", phone)
            .Build();

    public static ErrorDescriptor DuplicateEmail(string email) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Conflict.DuplicateEmail")
            .WithCategory(ErrorCategory.Conflict)
            .WithMessage("邮箱已被注册: {Email}")
            .AddContext("Email", email)
            .Build();
}

/// <summary>
/// Members 模块领域异常
/// Members module domain exception
/// </summary>
public sealed class MembersDomainException : ModuleDomainException
{
    public MembersDomainException(ErrorDescriptor errorDescriptor)
        : base(errorDescriptor)
    {
    }

    public MembersDomainException(ErrorDescriptor errorDescriptor, Exception innerException)
        : base(errorDescriptor, innerException)
    {
    }
}
