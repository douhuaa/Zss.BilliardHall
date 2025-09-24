using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Zss.BilliardHall.Shared;

namespace Zss.BilliardHall.Entities;

/// <summary>
/// 台球会话实体
/// </summary>
public class TableSession : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 台球桌ID
    /// </summary>
    public Guid BilliardTableId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 门店ID
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// 会话状态
    /// </summary>
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 计费时长（分钟）
    /// </summary>
    public int BillingMinutes { get; set; }

    /// <summary>
    /// 基础价格（元/小时）
    /// </summary>
    public decimal BaseHourlyRate { get; set; }

    /// <summary>
    /// 折扣金额
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 实际支付金额
    /// </summary>
    public decimal ActualPaidAmount { get; set; }

    /// <summary>
    /// 支付状态
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// 预约ID（如果是预约开台）
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }

    protected TableSession()
    {
    }

    public TableSession(
        Guid id,
        Guid billiardTableId,
        Guid userId,
        Guid storeId,
        decimal baseHourlyRate) : base(id)
    {
        BilliardTableId = billiardTableId;
        UserId = userId;
        StoreId = storeId;
        BaseHourlyRate = baseHourlyRate;
        StartTime = DateTime.UtcNow;
        Status = SessionStatus.Active;
        PaymentStatus = PaymentStatus.Pending;
    }

    /// <summary>
    /// 结束会话
    /// </summary>
    /// <param name="endTime">结束时间</param>
    public void EndSession(DateTime? endTime = null)
    {
        if (Status != SessionStatus.Active)
        {
            throw new InvalidOperationException($"会话状态为 {Status}，无法结束");
        }

        EndTime = endTime ?? DateTime.UtcNow;
        Status = SessionStatus.PendingSettlement;
        
        // 计算计费时长（分钟）
        var duration = EndTime.Value - StartTime;
        BillingMinutes = (int)Math.Ceiling(duration.TotalMinutes);
        
        // 计算总金额
        CalculateTotalAmount();
    }

    /// <summary>
    /// 完成支付
    /// </summary>
    /// <param name="paidAmount">实际支付金额</param>
    public void CompletePayment(decimal paidAmount)
    {
        if (Status != SessionStatus.PendingSettlement)
        {
            throw new InvalidOperationException($"会话状态为 {Status}，无法完成支付");
        }

        ActualPaidAmount = paidAmount;
        PaymentStatus = PaymentStatus.Success;
        Status = SessionStatus.Completed;
    }

    /// <summary>
    /// 取消会话
    /// </summary>
    public void CancelSession()
    {
        if (Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("已完成的会话无法取消");
        }

        Status = SessionStatus.Cancelled;
        EndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// 应用折扣
    /// </summary>
    /// <param name="discountAmount">折扣金额</param>
    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        CalculateTotalAmount();
    }

    /// <summary>
    /// 计算总金额
    /// </summary>
    private void CalculateTotalAmount()
    {
        // 基础计费：(分钟数 / 60) * 小时单价
        var baseAmount = (BillingMinutes / 60.0m) * BaseHourlyRate;
        TotalAmount = Math.Max(0, baseAmount - DiscountAmount);
    }

    /// <summary>
    /// 获取会话时长（分钟）
    /// </summary>
    /// <returns></returns>
    public int GetSessionDurationMinutes()
    {
        var endTime = EndTime ?? DateTime.UtcNow;
        return (int)Math.Ceiling((endTime - StartTime).TotalMinutes);
    }
}