namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 会员实体
/// </summary>
public class Member
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
