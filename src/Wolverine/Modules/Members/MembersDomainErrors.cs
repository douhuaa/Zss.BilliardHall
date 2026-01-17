using System.Net;
using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// Members 模块领域错误工厂（模块内部错误码定义）
/// </summary>
public static class MembersDomainErrors
{
    /// <summary>
    /// 会员不存在
    /// </summary>
    public static DomainException MemberNotFound()
        => new(new ErrorCode(
            "Members:NotFound",
            HttpStatusCode.NotFound,
            "会员不存在"));

    /// <summary>
    /// 会员已存在（手机号重复）
    /// </summary>
    public static DomainException MemberAlreadyExists(string phone)
        => new(new ErrorCode(
            "Members:AlreadyExists",
            HttpStatusCode.Conflict,
            $"手机号 {phone} 已被注册"));

    /// <summary>
    /// 余额不足
    /// </summary>
    public static DomainException InsufficientBalance(decimal required, decimal current)
        => new(new ErrorCode(
            "Members:InsufficientBalance",
            HttpStatusCode.BadRequest,
            $"余额不足，需要 {required:F2} 元，当前余额 {current:F2} 元"));

    /// <summary>
    /// 无效的会员等级
    /// </summary>
    public static DomainException InvalidTier(string tier)
        => new(new ErrorCode(
            "Members:InvalidTier",
            HttpStatusCode.BadRequest,
            $"无效的会员等级：{tier}"));

    /// <summary>
    /// 无效的手机号
    /// </summary>
    public static DomainException InvalidPhone(string phone)
        => new(new ErrorCode(
            "Members:InvalidPhone",
            HttpStatusCode.BadRequest,
            $"无效的手机号：{phone}"));

    /// <summary>
    /// 无效的金额
    /// </summary>
    public static DomainException InvalidAmount(decimal amount)
        => new(new ErrorCode(
            "Members:InvalidAmount",
            HttpStatusCode.BadRequest,
            $"无效的金额：{amount:F2} 元"));
}
