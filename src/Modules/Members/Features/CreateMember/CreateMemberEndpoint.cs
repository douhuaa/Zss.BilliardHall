namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员端点
/// 职责：HTTP 请求/响应处理，可以使用 Contracts
/// </summary>
public class CreateMemberEndpoint
{
    // 在实际实现中使用 Wolverine.HTTP 或 Minimal API
    // 示例：
    // public static async Task<IResult> Handle(
    //     CreateMemberCommand command, 
    //     IMessageBus bus)
    // {
    //     var memberId = await bus.InvokeAsync<Guid>(command);
    //     return Results.Created($"/members/{memberId}", new { Id = memberId });
    // }
}
