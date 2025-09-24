using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Zss.BilliardHall.Shared;

namespace Zss.BilliardHall.Entities;

/// <summary>
/// 台球桌实体
/// </summary>
public class BilliardTable : AuditedAggregateRoot<Guid>
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
    /// 台球桌状态
    /// </summary>
    public TableStatus Status { get; set; } = TableStatus.Idle;

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
    public decimal BasePrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 设备ID
    /// </summary>
    [MaxLength(100)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 二维码内容
    /// </summary>
    [MaxLength(500)]
    public string? QrCodeContent { get; set; }

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTime? LastHeartbeatTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }

    protected BilliardTable()
    {
    }

    public BilliardTable(
        Guid id,
        string tableNumber,
        string tableName,
        Guid storeId,
        decimal basePrice = 0) : base(id)
    {
        TableNumber = tableNumber;
        TableName = tableName;
        StoreId = storeId;
        BasePrice = basePrice;
        Status = TableStatus.Idle;
        IsEnabled = true;
    }

    /// <summary>
    /// 开台
    /// </summary>
    public void StartSession()
    {
        if (Status != TableStatus.Idle && Status != TableStatus.Reserved)
        {
            throw new InvalidOperationException($"台球桌状态为 {Status}，无法开台");
        }

        Status = TableStatus.InUse;
    }

    /// <summary>
    /// 关台
    /// </summary>
    public void EndSession()
    {
        if (Status != TableStatus.InUse)
        {
            throw new InvalidOperationException($"台球桌状态为 {Status}，无法关台");
        }

        Status = TableStatus.Idle;
    }

    /// <summary>
    /// 预约台球桌
    /// </summary>
    public void Reserve()
    {
        if (Status != TableStatus.Idle)
        {
            throw new InvalidOperationException($"台球桌状态为 {Status}，无法预约");
        }

        Status = TableStatus.Reserved;
    }

    /// <summary>
    /// 取消预约
    /// </summary>
    public void CancelReservation()
    {
        if (Status != TableStatus.Reserved)
        {
            throw new InvalidOperationException($"台球桌状态为 {Status}，无法取消预约");
        }

        Status = TableStatus.Idle;
    }

    /// <summary>
    /// 设置为维护状态
    /// </summary>
    public void SetMaintenance()
    {
        Status = TableStatus.Maintenance;
    }

    /// <summary>
    /// 设置为故障状态
    /// </summary>
    public void SetError()
    {
        Status = TableStatus.Error;
    }

    /// <summary>
    /// 更新心跳时间
    /// </summary>
    public void UpdateHeartbeat()
    {
        LastHeartbeatTime = DateTime.UtcNow;
    }
}