using System.ComponentModel.DataAnnotations;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 更改台球桌状态 DTO
/// </summary>
public class ChangeTableStatusDto
{
    /// <summary>
    /// 新状态
    /// </summary>
    [Required]
    public BilliardTableStatus Status { get; set; }
    
    /// <summary>
    /// 变更原因
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}