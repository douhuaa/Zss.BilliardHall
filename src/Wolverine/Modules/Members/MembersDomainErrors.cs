using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// 会员模块领域异常工厂
/// Factory for creating domain exceptions in Members module
/// </summary>
/// <remarks>
/// 提供统一的异常创建入口，确保异常消息格式一致且支持本地化
/// Provides unified exception creation to ensure consistent error message format and i18n support
/// </remarks>
public static class MembersDomainErrors
{
    public static DomainException NotFound(Guid memberId)
    {
        return new DomainException(
            MemberErrorCodes.NotFound.Code,
            $"会员不存在: {memberId}"
        );
    }

    public static DomainException DuplicatePhone(string phone)
    {
        return new DomainException(
            MemberErrorCodes.DuplicatePhone.Code,
            $"手机号已注册: {phone}"
        );
    }

    public static DomainException InsufficientBalance(Guid memberId, decimal required, decimal actual)
    {
        return new DomainException(
            MemberErrorCodes.InsufficientBalance.Code,
            $"余额不足: 需要 {required:F2} 元，实际 {actual:F2} 元"
        );
    }

    public static DomainException InvalidTopUpAmount(decimal amount)
    {
        return new DomainException(
            MemberErrorCodes.InvalidTopUpAmount.Code,
            $"充值金额无效: {amount:F2} 元"
        );
    }

    public static DomainException InvalidDeductAmount(decimal amount)
    {
        return new DomainException(
            MemberErrorCodes.InvalidDeductAmount.Code,
            $"扣除金额无效: {amount:F2} 元"
        );
    }

    public static DomainException InvalidAwardPoints(int points)
    {
        return new DomainException(
            MemberErrorCodes.InvalidAwardPoints.Code,
            $"无效的奖励积分: {points}"
        );
    }
}
