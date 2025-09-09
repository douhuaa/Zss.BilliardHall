using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Zss.BilliardHall.BilliardHalls;
using Zss.BilliardHall.Customers;

namespace Zss.BilliardHall.Reservations;

/// <summary>
/// 预约聚合根实体
/// </summary>
public class Reservation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// 预约编号
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ReservationNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户ID
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// 客户导航属性
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;
    
    /// <summary>
    /// 台球桌ID
    /// </summary>
    public Guid BilliardTableId { get; set; }
    
    /// <summary>
    /// 台球桌导航属性
    /// </summary>
    public virtual BilliardTable BilliardTable { get; set; } = null!;
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 预约时长（分钟）
    /// </summary>
    public int DurationMinutes { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// 预约状态
    /// </summary>
    public ReservationStatus Status { get; set; }
    
    /// <summary>
    /// 预约金额
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// 实际开始时间
    /// </summary>
    public DateTime? ActualStartTime { get; set; }
    
    /// <summary>
    /// 实际结束时间
    /// </summary>
    public DateTime? ActualEndTime { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(ReservationConsts.MaxNotesLength)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// 私有构造函数，用于 EF Core
    /// </summary>
    protected Reservation() { }
    
    /// <summary>
    /// 创建预约
    /// </summary>
    public Reservation(
        Guid id,
        string reservationNumber,
        Guid customerId,
        Guid billiardTableId,
        DateTime startTime,
        int durationMinutes,
        decimal amount,
        string? notes = null) : base(id)
    {
        ReservationNumber = Check.NotNullOrWhiteSpace(reservationNumber, nameof(reservationNumber));
        CustomerId = customerId;
        BilliardTableId = billiardTableId;
        
        SetReservationTime(startTime, durationMinutes);
        Amount = amount;
        Status = ReservationStatus.Pending;
        Notes = Check.Length(notes, nameof(notes), ReservationConsts.MaxNotesLength);
    }
    
    /// <summary>
    /// 设置预约时间
    /// </summary>
    public void SetReservationTime(DateTime startTime, int durationMinutes)
    {
        // 验证预约时间
        ValidateReservationTime(startTime, durationMinutes);
        
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndTime = startTime.AddMinutes(durationMinutes);
    }
    
    /// <summary>
    /// 确认预约
    /// </summary>
    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotCancelReservation)
                .WithData("Status", Status.ToString());
        }
        
        Status = ReservationStatus.Confirmed;
    }
    
    /// <summary>
    /// 开始使用
    /// </summary>
    public void StartUsing()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotCancelReservation)
                .WithData("Status", Status.ToString());
        }
        
        Status = ReservationStatus.InProgress;
        ActualStartTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 完成使用
    /// </summary>
    public void Complete()
    {
        if (Status != ReservationStatus.InProgress)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotCancelReservation)
                .WithData("Status", Status.ToString());
        }
        
        Status = ReservationStatus.Completed;
        ActualEndTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 取消预约
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == ReservationStatus.Completed || Status == ReservationStatus.Cancelled)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotCancelReservation)
                .WithData("Status", Status.ToString());
        }
        
        Status = ReservationStatus.Cancelled;
        if (!string.IsNullOrEmpty(reason))
        {
            Notes = Notes + (string.IsNullOrEmpty(Notes) ? "" : "; ") + $"取消原因: {reason}";
        }
    }
    
    /// <summary>
    /// 更新金额
    /// </summary>
    public void UpdateAmount(decimal amount)
    {
        if (amount >= 0)
        {
            Amount = amount;
        }
    }
    
    /// <summary>
    /// 设置备注
    /// </summary>
    public void SetNotes(string? notes)
    {
        if (notes != null && notes.Length > ReservationConsts.MaxNotesLength)
        {
            throw new ArgumentException($"Notes cannot exceed {ReservationConsts.MaxNotesLength} characters", nameof(notes));
        }
        Notes = notes;
    }
    
    /// <summary>
    /// 检查预约是否过期
    /// </summary>
    public bool IsExpired()
    {
        return Status == ReservationStatus.Confirmed && DateTime.UtcNow > StartTime.AddMinutes(30);
    }
    
    /// <summary>
    /// 验证预约时间
    /// </summary>
    private void ValidateReservationTime(DateTime startTime, int durationMinutes)
    {
        // 检查时长范围
        if (durationMinutes < ReservationConsts.MinDurationMinutes || durationMinutes > ReservationConsts.MaxDurationMinutes)
        {
            throw new ArgumentException($"Duration must be between {ReservationConsts.MinDurationMinutes} and {ReservationConsts.MaxDurationMinutes} minutes", nameof(durationMinutes));
        }
        
        // 检查预约时间不能在过去
        if (startTime <= DateTime.UtcNow)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.ReservationInPast);
        }
        
        // 检查预约时间不能太远（例如不超过30天）
        if (startTime > DateTime.UtcNow.AddDays(30))
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.ReservationTooFar);
        }
    }
}

/// <summary>
/// 预约状态枚举
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// 待确认
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// 已确认
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// 进行中
    /// </summary>
    InProgress = 3,
    
    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 5,
    
    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 6
}