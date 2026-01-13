using System.Collections;

namespace Zss.BilliardHall.Modules.Members.Events;

/// <summary>
/// 余额已扣减事件
/// Balance deducted event
/// </summary>
public sealed record BalanceDeducted(
    Guid MemberId,
    decimal Amount,
    string? Reason = null,
    DateTimeOffset DeductedAt = default
) : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
