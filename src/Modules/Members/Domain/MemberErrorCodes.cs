namespace Zss.BilliardHall.Modules.Members.Domain;

/// <summary>
/// 会员模块领域错误码常量
/// </summary>
public static class MemberErrorCodes
{
    /// <summary>
    /// 会员邮箱已存在
    /// </summary>
    public const string MemberEmailExists = "MEMBER_EMAIL_EXISTS";

    /// <summary>
    /// 会员手机号已存在
    /// </summary>
    public const string MemberPhoneNumberExists = "MEMBER_PHONE_NUMBER_EXISTS";
}
