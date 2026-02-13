namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令
/// 职责：表达业务意图，不包含业务逻辑
/// </summary>
public sealed record CreateMemberCommand(string Name, string Email, string? PhoneNumber);
