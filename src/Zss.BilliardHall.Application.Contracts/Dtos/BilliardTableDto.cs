using System.ComponentModel.DataAnnotations;
using Zss.BilliardHall.Shared;

namespace Zss.BilliardHall.Dtos;

/// <summary>
/// 台球桌信息DTO
/// </summary>
public class BilliardTableDto
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 台球桌编号
    /// </summary>
    public string TableNumber { get; set; } = string.Empty;

    /// <summary>
    /// 台球桌名称
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 台球桌状态
    /// </summary>
    public TableStatus Status { get; set; }

    /// <summary>
    /// 门店ID
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// 位置描述
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 基础价格（元/小时）
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 二维码内容
    /// </summary>
    public string? QrCodeContent { get; set; }

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTime? LastHeartbeatTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 创建台球桌DTO
/// </summary>
public class CreateBilliardTableDto
{
    /// <summary>
    /// 台球桌编号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TableNumber { get; set; } = string.Empty;

    /// <summary>
    /// 台球桌名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 门店ID
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// 位置描述
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// 基础价格（元/小时）
    /// </summary>
    [Range(0, 999.99)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    [MaxLength(100)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新台球桌DTO
/// </summary>
public class UpdateBilliardTableDto
{
    /// <summary>
    /// 台球桌名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 位置描述
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// 基础价格（元/小时）
    /// </summary>
    [Range(0, 999.99)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    [MaxLength(100)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }
}