using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Members;

public static class MemberErrorCodes
{
    public static readonly ErrorCode DuplicatePhone =
        new("Member.DuplicatePhone", "手机号已注册");

    public static readonly ErrorCode InvalidTopUpAmount =
        new("Member.InvalidTopUpAmount", "充值金额无效");

    public static readonly ErrorCode InvalidDeductAmount =
        new("Member.InvalidDeductAmount", "扣除金额无效");

    public static readonly ErrorCode InsufficientBalance =
        new("Member.InsufficientBalance", "余额不足");

    public static readonly ErrorCode InvalidAwardPoints =
        new("Member.InvalidAwardPoints", "无效的奖励积分");

    public static readonly ErrorCode NotFound =
        new("Member.NotFound", "会员未找到");            
}
