using System.Text.Json.Serialization;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// 会员聚合根
/// Member aggregate root
/// </summary>
public class Member
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public MemberTier Tier { get; private set; }
    public decimal Balance { get; private set; }
    public int Points { get; private set; }

    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? LastActiveAt { get; private set; }

    private Member() { }
    [JsonConstructor] // 显式告诉 System.Text.Json 用这个构造函数
    private Member(
        Guid id,
        string name,
        string phone,
        string email,
        MemberTier tier,
        decimal balance,
        int points,
        DateTimeOffset registeredAt,
        DateTimeOffset? lastActiveAt)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Email = email;
        Tier = tier;
        Balance = balance;
        Points = points;
        RegisteredAt = registeredAt == default ? DateTimeOffset.UtcNow : registeredAt;
        LastActiveAt = lastActiveAt;
    }
    internal static Member CreateInstance(RegisterMember.RegisterMember command)
    {
        return new Member { Id = Guid.CreateVersion7(),Name = command.Name, Phone = command.Phone, Email = command.Email, Tier = MemberTier.Regular, Balance = 0, Points = 0, RegisteredAt = DateTimeOffset.UtcNow };
    }
    public static Member CreateInstance(Guid id, string name, string phone, string email, MemberTier tier = MemberTier.Regular, decimal balance=0, int points=0, DateTimeOffset registeredAt = default, DateTimeOffset? lastActiveAt = null)
    {
        return new Member { Id = id, Name = name, Phone = phone, Email = email, Tier = tier, Balance = balance, Points = points, RegisteredAt = registeredAt, LastActiveAt = lastActiveAt };
    }
    // EF

    /// <summary>
    /// 充值余额
    /// Top up balance
    /// </summary>
    public void TopUp(decimal amount)
    {
        if (amount <= 0)
            throw MembersDomainErrors.InvalidAmount(amount);

        Balance += amount;
    }

    /// <summary>
    /// 扣减余额
    /// Deduct balance
    /// </summary>
    public void Deduct(decimal amount)
    {
        if (amount <= 0)
            throw MembersDomainErrors.InvalidAmount(amount);

        if (Balance < amount)
            throw MembersDomainErrors.InsufficientBalance(amount, Balance);

        Balance -= amount;
        Touch();
    }

    /// <summary>
    /// 赠送积分
    /// Award points
    /// </summary>
    public void AwardPoints(int points)
    {
        if (points <= 0)
            throw MembersDomainErrors.InvalidAmount(points);

        var previousTier = Tier;

        Points += points;
        CheckTierUpgrade();

        if (Tier != previousTier)
        {   // TODO(cascading): Emit MemberTierUpgraded event via Wolverine cascading messages
            // when wiring Members module handlers. See docs/06_开发规范/级联消息与副作用.md
            // and the corresponding Members module issue tracking event integration.
            // RaiseDomainEvent(new MemberTierUpgraded(Id, previousTier, Tier));
        }
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

    private void Touch()
    {
        LastActiveAt = DateTimeOffset.UtcNow;
    }
}

