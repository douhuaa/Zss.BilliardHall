namespace Zss.BilliardHall.Modules.Members.RegisterMember;

/// <summary>
/// 注册会员命令
/// Register member command
/// </summary>
public sealed record RegisterMember(
    string Name,
    string Phone,
    string Email,
    string Password
);
