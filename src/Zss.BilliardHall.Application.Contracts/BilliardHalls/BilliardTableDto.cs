using System;
using Volo.Abp.Application.Dtos;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌 DTO
/// </summary>
public class BilliardTableDto : FullAuditedEntityDto<Guid>
{
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
    public string? Description { get; set; }
    
    /// <summary>
    /// 所属台球厅ID
    /// </summary>
    public Guid BilliardHallId { get; set; }
    
    /// <summary>
    /// 是否可用
    /// </summary>
    public bool IsAvailable => Status == BilliardTableStatus.Available;
}