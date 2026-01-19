namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员命令
/// 职责：表达业务意图，不包含业务逻辑
/// </summary>
public record CreateMemberCommand
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
}
