using Microsoft.AspNetCore.Http;
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
    public static DomainException NotFound() => new(MemberErrorCodes.NotFound);

    public static DomainException DuplicatePhone(string phone) => new(MemberErrorCodes.DuplicatePhone);

    public static DomainException InsufficientBalance(Guid memberId, decimal required, decimal actual) =>
        new(MemberErrorCodes.InsufficientBalance);

    public static DomainException InvalidTopUpAmount(decimal amount) => new(MemberErrorCodes.InvalidTopUpAmount);

    public static DomainException InvalidDeductAmount(decimal amount) => new(MemberErrorCodes.InvalidDeductAmount);

    public static DomainException InvalidAwardPoints(int points) => new(MemberErrorCodes.InvalidAwardPoints);
}
public static class MemberErrorCodes
{
    public static readonly ErrorCode DuplicatePhone = new(
        "Member.Conflict.DuplicatePhone",
        StatusCodes.Status409Conflict,
        "手机号已注册"
    );

    public static readonly ErrorCode InvalidTopUpAmount = new(
        "Member.Validation.InvalidTopUpAmount",
        StatusCodes.Status400BadRequest,
        "充值金额无效"
    );

    public static readonly ErrorCode InvalidDeductAmount = new(
        "Member.Validation.InvalidDeductAmount",
        StatusCodes.Status400BadRequest,
        "扣除金额无效"
    );

    public static readonly ErrorCode InvalidAwardPoints = new(
        "Member.Validation.InvalidAwardPoints",
        StatusCodes.Status400BadRequest,
        "无效的奖励积分"
    );

    public static readonly ErrorCode InsufficientBalance = new(
        "Member.Conflict.InsufficientBalance",
        StatusCodes.Status409Conflict,
        "余额不足"
    );

    public static readonly ErrorCode NotFound = new(
        "Member.NotFound.Member",
        StatusCodes.Status404NotFound,
        "会员未找到"
    );
}