using System.Text.Json.Serialization;
using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// 会员聚合根
/// Member aggregate root
/// </summary>
public class Member
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }

    public MemberTier Tier { get; private set; }
    public decimal Balance { get; private set; }
    public int Points { get; private set; }


    // 给 Marten/System.Text.Json 使用的构造器
    [JsonConstructor]
    private Member(Guid id, string name, string phone, string email, MemberTier tier, decimal balance, int points)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Email = email;
        Tier = tier;
        Balance = balance;
        Points = points;
    }


    public static Member Register(string name, string phone, string email, MemberTier tier = MemberTier.Regular, decimal balance = 0, int points = 0)
    {
        // 聚合内部可以做最基本的不变量校验
        return new Member(Guid.CreateVersion7(), name, phone, email, tier, balance, points);
    }
    
    /// <summary>
    /// 充值余额
    /// Top up balance
    /// </summary>
    public void TopUp(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException(MemberErrorCodes.InvalidTopUpAmount);
        Balance += amount;
    }

    /// <summary>
    /// 扣减余额
    /// Deduct balance
    /// </summary>
    public void Deduct(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException(MemberErrorCodes.InvalidDeductAmount);

        if (Balance < amount)
            throw new DomainException(MemberErrorCodes.InsufficientBalance);
        
        Balance -= amount;
    }

    /// <summary>
    /// 赠送积分
    /// Award points
    /// </summary>
    public void AwardPoints(int points)
    {
        if (points <= 0)
            throw new DomainException(MemberErrorCodes.InvalidAwardPoints);

        var previousTier = Tier;

        Points += points;
        RecalculateTier();
    }

    /// <summary>
    /// 检查并自动升级会员等级
    /// Check and automatically upgrade member tier
    /// </summary>
    private void RecalculateTier()
    {
        var newTier = Points switch
        {
            >= 10000 => MemberTier.Platinum,
            >= 5000 => MemberTier.Gold,
            >= 1000 => MemberTier.Silver,
            _ => MemberTier.Regular,
        };

        if (newTier > Tier)
        {
            Tier = newTier;
        }
    }
}