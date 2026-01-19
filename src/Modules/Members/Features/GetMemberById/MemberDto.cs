namespace Zss.BilliardHall.Modules.Members.Features.GetMemberById;

/// <summary>
/// 会员 DTO（数据传输对象）
/// 注意：这是模块内部的 DTO，不是跨模块的 Contract
/// 如果需要暴露给其他模块，应该定义在 Platform.Contracts 中
/// </summary>
public record MemberDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime CreatedAt { get; init; }
}
