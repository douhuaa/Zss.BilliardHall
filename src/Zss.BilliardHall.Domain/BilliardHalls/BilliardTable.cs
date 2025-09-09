using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌实体
/// </summary>
public class BilliardTable : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// 台球桌编号
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// 台球桌类型
    /// </summary>
    public BilliardTableType Type { get; set; }
    
    /// <summary>
    /// 当前状态
    /// </summary>
    public BilliardTableStatus Status { get; set; }
    
    /// <summary>
    /// 小时费率
    /// </summary>
    public decimal HourlyRate { get; set; }
    
    /// <summary>
    /// X 坐标位置
    /// </summary>
    public float LocationX { get; set; }
    
    /// <summary>
    /// Y 坐标位置
    /// </summary>
    public float LocationY { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    [StringLength(BilliardTableConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 所属台球厅ID
    /// </summary>
    public Guid BilliardHallId { get; set; }
    
    /// <summary>
    /// 台球厅导航属性
    /// </summary>
    public virtual BilliardHall BilliardHall { get; set; } = null!;
    
    /// <summary>
    /// 私有构造函数，用于 EF Core
    /// </summary>
    protected BilliardTable() { }
    
    /// <summary>
    /// 创建台球桌
    /// </summary>
    internal BilliardTable(
        Guid id,
        int number,
        BilliardTableType type,
        decimal hourlyRate,
        float locationX = 0,
        float locationY = 0,
        string? description = null) : base(id)
    {
        Number = number > 0 ? number : throw new ArgumentException("Number must be positive", nameof(number));
        Type = type;
        Status = BilliardTableStatus.Available;
        HourlyRate = Check.Range(hourlyRate, nameof(hourlyRate), BilliardTableConsts.MinHourlyRate, BilliardTableConsts.MaxHourlyRate);
        LocationX = locationX;
        LocationY = locationY;
        Description = description;
    }
    
    /// <summary>
    /// 更改台球桌状态
    /// </summary>
    public void ChangeStatus(BilliardTableStatus newStatus)
    {
        // 业务规则验证
        ValidateStatusTransition(Status, newStatus);
        Status = newStatus;
    }
    
    /// <summary>
    /// 更新小时费率
    /// </summary>
    public void UpdateHourlyRate(decimal newRate)
    {
        HourlyRate = newRate >= BilliardTableConsts.MinHourlyRate && newRate <= BilliardTableConsts.MaxHourlyRate 
            ? newRate 
            : throw new ArgumentException("Hourly rate out of range", nameof(newRate));
    }
    
    /// <summary>
    /// 更新位置
    /// </summary>
    public void UpdateLocation(float x, float y)
    {
        LocationX = x;
        LocationY = y;
    }
    
    /// <summary>
    /// 设置描述
    /// </summary>
    public void SetDescription(string? description)
    {
        if (description != null && description.Length > BilliardTableConsts.MaxDescriptionLength)
        {
            throw new ArgumentException($"Description cannot exceed {BilliardTableConsts.MaxDescriptionLength} characters", nameof(description));
        }
        Description = description;
    }
    
    /// <summary>
    /// 检查台球桌是否可用
    /// </summary>
    public bool IsAvailable()
    {
        return Status == BilliardTableStatus.Available;
    }
    
    /// <summary>
    /// 验证状态转换是否有效
    /// </summary>
    private void ValidateStatusTransition(BilliardTableStatus from, BilliardTableStatus to)
    {
        // 从故障状态只能转换到维护状态
        if (from == BilliardTableStatus.OutOfOrder && to != BilliardTableStatus.Maintenance)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotChangeStatusFromOutOfOrder);
        }
        
        // 从占用状态不能直接转换到预约状态
        if (from == BilliardTableStatus.Occupied && to == BilliardTableStatus.Reserved)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.TableNotAvailable)
                .WithData("CurrentStatus", from.ToString())
                .WithData("TargetStatus", to.ToString());
        }
    }
}