using System;
using System.ComponentModel.DataAnnotations;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 创建台球桌 DTO
/// </summary>
public class CreateBilliardTableDto
{
    /// <summary>
    /// 台球桌编号
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "台球桌编号必须大于0")]
    public int Number { get; set; }
    
    /// <summary>
    /// 台球桌类型
    /// </summary>
    [Required]
    public BilliardTableType Type { get; set; }
    
    /// <summary>
    /// 小时费率
    /// </summary>
    [Required]
    [Range(typeof(decimal), "0.01", "9999.99", ErrorMessage = "小时费率必须在0.01-9999.99之间")]
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
    [Required]
    public Guid BilliardHallId { get; set; }
}