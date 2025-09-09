using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Guids;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球厅聚合根实体
/// </summary>
public class BilliardHall : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly IGuidGenerator _guidGenerator;
    public Guid? TenantId { get; set; }
    
    [Required]
    [StringLength(BilliardHallConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(BilliardHallConsts.MaxAddressLength)]
    public string? Address { get; set; }
    
    [StringLength(BilliardHallConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    [StringLength(BilliardHallConsts.MaxPhoneLength)]
    public string? Phone { get; set; }
    
    [StringLength(BilliardHallConsts.MaxEmailLength)]
    public string? Email { get; set; }
    
    /// <summary>
    /// 开门时间
    /// </summary>
    public TimeSpan OpenTime { get; set; }
    
    /// <summary>
    /// 关门时间
    /// </summary>
    public TimeSpan CloseTime { get; set; }
    
    /// <summary>
    /// 是否营业
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// 台球桌列表
    /// </summary>
    public virtual ICollection<BilliardTable> Tables { get; set; }
    
    /// <summary>
    /// 私有构造函数，用于 EF Core
    /// </summary>
    protected BilliardHall()
    {
        Tables = new List<BilliardTable>();
        _guidGenerator = SimpleGuidGenerator.Instance;
    }
    
    /// <summary>
    /// 创建台球厅
    /// </summary>
    public BilliardHall(
        Guid id,
        string name,
        string? address = null,
        string? description = null,
        string? phone = null,
        string? email = null,
        TimeSpan? openTime = null,
        TimeSpan? closeTime = null,
        IGuidGenerator? guidGenerator = null) : base(id)
    {
        SetName(name);
        Address = address;
        Description = description;
        Phone = phone;
        Email = email;
        OpenTime = openTime ?? TimeSpan.FromHours(9);
        CloseTime = closeTime ?? TimeSpan.FromHours(22);
        IsActive = true;
        Tables = new List<BilliardTable>();
        _guidGenerator = guidGenerator ?? SimpleGuidGenerator.Instance;
    }
    
    /// <summary>
    /// 设置台球厅名称
    /// </summary>
    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), BilliardHallConsts.MaxNameLength);
    }
    
    /// <summary>
    /// 添加台球桌
    /// </summary>
    public BilliardTable AddTable(int number, BilliardTableType type, decimal hourlyRate, float locationX = 0, float locationY = 0)
    {
        if (number <= 0)
        {
            throw new ArgumentException("Table number must be positive", nameof(number));
        }
        if (hourlyRate < BilliardTableConsts.MinHourlyRate || hourlyRate > BilliardTableConsts.MaxHourlyRate)
        {
            throw new ArgumentException("Hourly rate out of range", nameof(hourlyRate));
        }
        
        if (Tables.Any(t => t.Number == number))
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists)
                .WithData("Number", number);
        }
        
        var table = new BilliardTable(
            _guidGenerator.Create(),
            number,
            type,
            hourlyRate,
            locationX,
            locationY
        );
        
        Tables.Add(table);
        return table;
    }
    
    /// <summary>
    /// 移除台球桌
    /// </summary>
    public void RemoveTable(int number)
    {
        var table = Tables.FirstOrDefault(t => t.Number == number);
        if (table != null)
        {
            Tables.Remove(table);
        }
    }
    
    /// <summary>
    /// 更新营业时间
    /// </summary>
    public void UpdateBusinessHours(TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Close time must be after open time", nameof(closeTime));
        }
        
        OpenTime = openTime;
        CloseTime = closeTime;
    }
    
    /// <summary>
    /// 设置营业状态
    /// </summary>
    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
}