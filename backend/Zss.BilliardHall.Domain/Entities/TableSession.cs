using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace Zss.BilliardHall.Entities
{
    /// <summary>
    /// 台桌会话实体
    /// </summary>
    public class TableSession : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 台桌ID
        /// </summary>
        public Guid TableId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 会话令牌（用于幂等性）
        /// </summary>
        [StringLength(100)]
        public string SessionToken { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 会话状态
        /// </summary>
        public SessionStatus Status { get; set; }

        /// <summary>
        /// 总时长（分钟）
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// 小时费率快照
        /// </summary>
        public decimal HourlyRateSnapshot { get; set; }

        /// <summary>
        /// 计算金额
        /// </summary>
        public decimal CalculatedAmount { get; set; }

        /// <summary>
        /// 实际支付金额
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 关联的台桌
        /// </summary>
        public virtual BilliardTable Table { get; set; }

        protected TableSession()
        {
        }

        public TableSession(Guid id, Guid tableId, Guid userId, string sessionToken, decimal hourlyRate)
            : base(id)
        {
            TableId = tableId;
            UserId = userId;
            SessionToken = sessionToken;
            StartTime = DateTime.UtcNow;
            Status = SessionStatus.Active;
            HourlyRateSnapshot = hourlyRate;
            DurationMinutes = 0;
            CalculatedAmount = 0;
            PaidAmount = 0;
        }

        public void EndSession(DateTime endTime)
        {
            if (Status != SessionStatus.Active)
            {
                throw new InvalidOperationException("只有进行中的会话可以结束");
            }

            EndTime = endTime;
            DurationMinutes = (int)(endTime - StartTime).TotalMinutes;
            CalculatedAmount = CalculateAmount();
            Status = SessionStatus.Frozen;
        }

        public void CompletePayment(decimal paidAmount)
        {
            if (Status != SessionStatus.Frozen)
            {
                throw new InvalidOperationException("只有冻结状态的会话可以完成支付");
            }

            PaidAmount = paidAmount;
            Status = SessionStatus.Completed;
        }

        public void Cancel()
        {
            Status = SessionStatus.Cancelled;
            EndTime = DateTime.UtcNow;
        }

        public decimal CalculateAmount()
        {
            if (DurationMinutes <= 0)
            {
                return 0;
            }

            // 按分钟计费，不足一小时按一小时算
            var hours = Math.Ceiling(DurationMinutes / 60.0m);
            return hours * HourlyRateSnapshot;
        }

        public void UpdateDuration()
        {
            if (Status == SessionStatus.Active && StartTime != default)
            {
                DurationMinutes = (int)(DateTime.UtcNow - StartTime).TotalMinutes;
                CalculatedAmount = CalculateAmount();
            }
        }
    }
}