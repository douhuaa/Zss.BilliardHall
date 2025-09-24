using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace Zss.BilliardHall.Entities
{
    /// <summary>
    /// 支付订单实体
    /// </summary>
    public class PaymentOrder : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        /// <summary>
        /// 应付金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 实付金额
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// 第三方支付交易号
        /// </summary>
        [StringLength(100)]
        public string ThirdPartyTransactionId { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 支付回调数据
        /// </summary>
        [StringLength(2000)]
        public string CallbackData { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 关联的会话
        /// </summary>
        public virtual TableSession Session { get; set; }

        protected PaymentOrder()
        {
        }

        public PaymentOrder(Guid id, Guid sessionId, Guid userId, decimal amount, PaymentMethod paymentMethod)
            : base(id)
        {
            SessionId = sessionId;
            UserId = userId;
            Amount = amount;
            PaidAmount = 0;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;
            OrderNumber = GenerateOrderNumber();
            ExpirationTime = DateTime.UtcNow.AddMinutes(15); // 15分钟过期
        }

        public void MarkAsPaid(decimal paidAmount, string thirdPartyTransactionId = null, string callbackData = null)
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("只有待支付状态的订单可以标记为已支付");
            }

            PaidAmount = paidAmount;
            ThirdPartyTransactionId = thirdPartyTransactionId;
            CallbackData = callbackData;
            PaymentTime = DateTime.UtcNow;
            Status = PaymentStatus.Paid;
        }

        public void MarkAsFailed(string reason = null)
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("只有待支付状态的订单可以标记为失败");
            }

            Status = PaymentStatus.Failed;
            Remarks = reason;
        }

        public void Cancel()
        {
            if (Status == PaymentStatus.Paid)
            {
                throw new InvalidOperationException("已支付的订单不能取消");
            }

            Status = PaymentStatus.Cancelled;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpirationTime && Status == PaymentStatus.Pending;
        }

        private string GenerateOrderNumber()
        {
            return $"BH{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }

        public void Extend(int minutes)
        {
            if (Status == PaymentStatus.Pending)
            {
                ExpirationTime = ExpirationTime.AddMinutes(minutes);
            }
        }
    }
}