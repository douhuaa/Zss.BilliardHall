using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Modules.Members.Exceptions;

/// <summary>
/// 会员手机号已存在异常
/// 当尝试创建重复手机号的会员时抛出此领域异常
/// </summary>
public sealed class MemberPhoneNumberAlreadyExistsException : DomainException
{
    public MemberPhoneNumberAlreadyExistsException()
        : base(MemberErrorCodes.MemberPhoneNumberExists, "会员手机号已存在")
    {
    }

    public MemberPhoneNumberAlreadyExistsException(Exception innerException)
        : base(MemberErrorCodes.MemberPhoneNumberExists, "会员手机号已存在", innerException)
    {
    }
}
