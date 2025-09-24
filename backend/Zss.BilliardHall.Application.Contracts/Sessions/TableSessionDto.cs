using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Zss.BilliardHall.Sessions
{
    /// <summary>
    /// 会话信息DTO
    /// </summary>
    public class TableSessionDto : EntityDto<Guid>
    {
        /// <summary>
        /// 台桌ID
        /// </summary>
        public Guid TableId { get; set; }

        /// <summary>
        /// 台桌编号
        /// </summary>
        public string TableNumber { get; set; }

        /// <summary>
        /// 台桌名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 会话令牌
        /// </summary>
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
        /// 状态显示名称
        /// </summary>
        public string StatusDisplayName { get; set; }

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
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 开始会话DTO
    /// </summary>
    public class StartSessionDto
    {
        /// <summary>
        /// 台桌ID
        /// </summary>
        [Required]
        public Guid TableId { get; set; }

        /// <summary>
        /// 会话令牌（用于幂等性）
        /// </summary>
        [StringLength(100)]
        public string SessionToken { get; set; }
    }

    /// <summary>
    /// 结束会话DTO
    /// </summary>
    public class EndSessionDto
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        [Required]
        public Guid SessionId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 扫码开台DTO
    /// </summary>
    public class ScanToStartSessionDto
    {
        /// <summary>
        /// 二维码内容
        /// </summary>
        [Required]
        [StringLength(500)]
        public string QrCode { get; set; }

        /// <summary>
        /// 会话令牌（用于幂等性）
        /// </summary>
        [StringLength(100)]
        public string SessionToken { get; set; }
    }

    /// <summary>
    /// 会话查询参数DTO
    /// </summary>
    public class GetSessionsInput : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 台桌ID过滤
        /// </summary>
        public Guid? TableId { get; set; }

        /// <summary>
        /// 用户ID过滤
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 状态过滤
        /// </summary>
        public SessionStatus? Status { get; set; }

        /// <summary>
        /// 开始日期过滤
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期过滤
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}