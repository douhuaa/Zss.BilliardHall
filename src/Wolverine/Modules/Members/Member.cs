namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// 会员聚合根
/// Member aggregate root
/// </summary>
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MemberTier Tier { get; set; }
    public decimal Balance { get; set; }
    public int Points { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }

    /// <summary>
    /// 充值余额
    /// Top up balance
    /// </summary>
    public void TopUp(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("充值金额必须大于0");

        Balance += amount;
    }

    /// <summary>
    /// 扣减余额
    /// Deduct balance
    /// </summary>
    public void Deduct(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("扣减金额必须大于0");

        if (Balance < amount)
            throw new InvalidOperationException("余额不足");

        Balance -= amount;
    }

    /// <summary>
    /// 赠送积分
    /// Award points
    /// </summary>
    public void AwardPoints(int points)
    {
        if (points <= 0)
            throw new InvalidOperationException("积分必须大于0");

        Points += points;
        CheckTierUpgrade();
    }

    /// <summary>
    /// 检查并自动升级会员等级
    /// Check and automatically upgrade member tier
    /// </summary>
    private void CheckTierUpgrade()
    {
        var newTier = Points switch
        {
            >= 10000 => MemberTier.Platinum,
            >= 5000 => MemberTier.Gold,
            >= 1000 => MemberTier.Silver,
            _ => MemberTier.Regular
        };

        if (newTier > Tier)
        {
            Tier = newTier;
        }
    }
}
