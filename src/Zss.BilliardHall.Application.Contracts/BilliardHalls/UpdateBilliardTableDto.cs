using System.ComponentModel.DataAnnotations;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 更新台球桌 DTO
/// </summary>
public class UpdateBilliardTableDto
{
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
}