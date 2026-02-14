namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员端点
/// 职责：HTTP 请求/响应处理
/// </summary>
public static class CreateMemberEndpoint
{
    [WolverinePost("/api/members")]
    public static Task<Guid> Create(CreateMemberCommand command, IMessageBus bus, CancellationToken ct = default) => bus.InvokeAsync<Guid>(command, ct);
}
