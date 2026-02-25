using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Platform.Exceptions;

namespace Zss.BilliardHall.Modules.Members.Exceptions;

/// <summary>
/// 会员邮箱已存在异常
/// 当尝试创建重复邮箱的会员时抛出此领域异常
/// </summary>
public sealed class MemberEmailAlreadyExistsException()
    : DomainException(MemberErrorCodes.MemberEmailExists, "会员邮箱已存在");
