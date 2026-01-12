using System.Text.Json.Serialization;
using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// 会员聚合根
/// Member aggregate root
/// </summary>
public class Member : IFullAuditedEntity
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

    // 审计字段 (Audit Fields)
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

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
        DateTimeOffset? lastActiveAt,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        string? createdBy,
        string? updatedBy)
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
        CreatedAt = createdAt == default ? DateTimeOffset.UtcNow : createdAt;
        UpdatedAt = updatedAt;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }
    internal static Member CreateInstance(RegisterMember.RegisterMember command)
    {
        var now = DateTimeOffset.UtcNow;
        return new Member 
        { 
            Id = Guid.CreateVersion7(),
            Name = command.Name, 
            Phone = command.Phone, 
            Email = command.Email, 
            Tier = MemberTier.Regular, 
            Balance = 0, 
            Points = 0, 
            RegisteredAt = now,
            CreatedAt = now
        };
    }
    public static Member CreateInstance(
        Guid id, 
        string name, 
        string phone, 
        string email, 
        MemberTier tier = MemberTier.Regular, 
        decimal balance = 0, 
        int points = 0, 
        DateTimeOffset registeredAt = default, 
        DateTimeOffset? lastActiveAt = null,
        DateTimeOffset createdAt = default,
        DateTimeOffset? updatedAt = null,
        string? createdBy = null,
        string? updatedBy = null)
    {
        return new Member 
        { 
            Id = id, 
            Name = name, 
            Phone = phone, 
            Email = email, 
            Tier = tier, 
            Balance = balance, 
            Points = points, 
            RegisteredAt = registeredAt == default ? DateTimeOffset.UtcNow : registeredAt, 
            LastActiveAt = lastActiveAt,
            CreatedAt = createdAt == default ? DateTimeOffset.UtcNow : createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy
        };
    }
    // EF

    /// <summary>
    /// 充值余额
    /// Top up balance
    /// </summary>
    public DomainResult TopUp(decimal amount)
    {
        if (amount <= 0)
            return DomainResult.Fail(MemberErrorCodes.InvalidTopUpAmount);

        Balance += amount;
        UpdateAuditInfo();
        return DomainResult.Success();
    }

    /// <summary>
    /// 扣减余额
    /// Deduct balance
    /// </summary>
    public DomainResult Deduct(decimal amount)
    {
        if (amount <= 0)
            return DomainResult.Fail(MemberErrorCodes.InvalidDeductAmount);

        if (Balance < amount)
            return DomainResult.Fail(MemberErrorCodes.InsufficientBalance);

        Balance -= amount;
        Touch();
        UpdateAuditInfo();

        return DomainResult.Success();
    }

    /// <summary>
    /// 赠送积分
    /// Award points
    /// </summary>
    public DomainResult AwardPoints(int points)
    {
        if (points <= 0)
            return DomainResult.Fail(MemberErrorCodes.InvalidAwardPoints);

        var previousTier = Tier;

        Points += points;
        CheckTierUpgrade();
        UpdateAuditInfo();

        if (Tier != previousTier)
        {   // TODO(cascading): Emit MemberTierUpgraded event via Wolverine cascading messages
            // when wiring Members module handlers. See doc/06_开发规范/级联消息与副作用.md
            // and the corresponding Members module issue tracking event integration.
            // RaiseDomainEvent(new MemberTierUpgraded(Id, previousTier, Tier));
        }
        return DomainResult.Success();
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

    /// <summary>
    /// 更新审计信息
    /// Update audit information
    /// </summary>
    /// <param name="userId">当前用户ID（可选）</param>
    public void UpdateAuditInfo(string? userId = null)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        if (userId != null)
        {
            UpdatedBy = userId;
        }
    }

    /// <summary>
    /// 设置创建人信息（在创建时使用）
    /// Set creator information (used during creation)
    /// </summary>
    /// <param name="userId">创建人ID</param>
    public void SetCreator(string userId)
    {
        CreatedBy = userId;
    }
}

