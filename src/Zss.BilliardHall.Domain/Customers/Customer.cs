using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Zss.BilliardHall.BilliardHalls;

namespace Zss.BilliardHall.Customers;

/// <summary>
/// 客户实体
/// </summary>
public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// 客户姓名
    /// </summary>
    [Required]
    [StringLength(CustomerConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 手机号码
    /// </summary>
    [Required]
    [StringLength(CustomerConsts.MaxPhoneLength)]
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [StringLength(CustomerConsts.MaxEmailLength)]
    public string? Email { get; set; }
    
    /// <summary>
    /// 会员等级
    /// </summary>
    public CustomerLevel Level { get; set; }
    
    /// <summary>
    /// 积分
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// 余额
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(CustomerConsts.MaxNotesLength)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisterTime { get; set; }
    
    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastVisitTime { get; set; }
    
    /// <summary>
    /// 私有构造函数，用于 EF Core
    /// </summary>
    protected Customer() { }
    
    /// <summary>
    /// 创建客户
    /// </summary>
    public Customer(
        Guid id,
        string name,
        string phone,
        string? email = null,
        CustomerLevel level = CustomerLevel.Regular,
        string? notes = null) : base(id)
    {
        SetName(name);
        SetPhone(phone);
        Email = email;
        Level = level;
        Points = 0;
        Balance = 0;
        IsActive = true;
        Notes = notes;
        RegisterTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 设置客户姓名
    /// </summary>
    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), CustomerConsts.MaxNameLength);
    }
    
    /// <summary>
    /// 设置手机号码
    /// </summary>
    public void SetPhone(string phone)
    {
        Phone = Check.NotNullOrWhiteSpace(phone, nameof(phone), CustomerConsts.MaxPhoneLength);
    }
    
    /// <summary>
    /// 设置邮箱
    /// </summary>
    public void SetEmail(string? email)
    {
        if (email != null && email.Length > CustomerConsts.MaxEmailLength)
        {
            throw new ArgumentException($"Email cannot exceed {CustomerConsts.MaxEmailLength} characters", nameof(email));
        }
        Email = email;
    }
    
    /// <summary>
    /// 更新会员等级
    /// </summary>
    public void UpdateLevel(CustomerLevel level)
    {
        Level = level;
    }
    
    /// <summary>
    /// 增加积分
    /// </summary>
    public void AddPoints(int points)
    {
        if (points > 0)
        {
            Points += points;
            
            // 根据积分自动升级会员等级
            AutoUpgradeLevel();
        }
    }
    
    /// <summary>
    /// 扣减积分
    /// </summary>
    public void DeductPoints(int points)
    {
        if (points > 0 && Points >= points)
        {
            Points -= points;
        }
    }
    
    /// <summary>
    /// 充值
    /// </summary>
    public void Recharge(decimal amount)
    {
        if (amount > 0)
        {
            Balance += amount;
        }
    }
    
    /// <summary>
    /// 消费
    /// </summary>
    public bool Consume(decimal amount)
    {
        if (amount > 0 && Balance >= amount)
        {
            Balance -= amount;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 更新最后访问时间
    /// </summary>
    public void UpdateLastVisitTime()
    {
        LastVisitTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 激活/停用客户
    /// </summary>
    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
    
    /// <summary>
    /// 设置备注
    /// </summary>
    public void SetNotes(string? notes)
    {
        if (notes != null && notes.Length > CustomerConsts.MaxNotesLength)
        {
            throw new ArgumentException($"Notes cannot exceed {CustomerConsts.MaxNotesLength} characters", nameof(notes));
        }
        Notes = notes;
    }
    
    /// <summary>
    /// 根据积分自动升级会员等级
    /// </summary>
    private void AutoUpgradeLevel()
    {
        var newLevel = Points switch
        {
            >= 10000 => CustomerLevel.Diamond,
            >= 5000 => CustomerLevel.Gold,
            >= 2000 => CustomerLevel.Silver,
            >= 500 => CustomerLevel.Bronze,
            _ => CustomerLevel.Regular
        };
        
        if (newLevel > Level)
        {
            Level = newLevel;
        }
    }
}

/// <summary>
/// 客户等级枚举
/// </summary>
public enum CustomerLevel
{
    /// <summary>
    /// 普通会员
    /// </summary>
    Regular = 1,
    
    /// <summary>
    /// 铜牌会员
    /// </summary>
    Bronze = 2,
    
    /// <summary>
    /// 银牌会员
    /// </summary>
    Silver = 3,
    
    /// <summary>
    /// 金牌会员
    /// </summary>
    Gold = 4,
    
    /// <summary>
    /// 钻石会员
    /// </summary>
    Diamond = 5
}