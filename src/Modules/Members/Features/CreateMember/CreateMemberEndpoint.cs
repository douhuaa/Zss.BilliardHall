namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

/// <summary>
/// 创建会员端点
/// 职责：HTTP 请求/响应处理
/// </summary>
public static class CreateMemberEndpoint
{

    [WolverinePost("/api/members")] //不能自动提交事务，必须手动调用 SaveChangesAsync()，不推荐使用
    public static CreateMemberCommand Create(CreateMemberCommand command) => command;

    [WolverinePost("/api/v2/members")]
    public static Task<Guid> CreateV2(CreateMemberCommand command, IMessageBus bus, CancellationToken ct = default) => bus.InvokeAsync<Guid>(command, ct);
}
