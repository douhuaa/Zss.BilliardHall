using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Zss.BilliardHall.Tables
{
    /// <summary>
    /// 台桌信息DTO
    /// </summary>
    public class BilliardTableDto : EntityDto<Guid>
    {
        /// <summary>
        /// 门店ID
        /// </summary>
        public Guid StoreId { get; set; }

        /// <summary>
        /// 台桌编号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 台桌名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 台桌状态
        /// </summary>
        public TableStatus Status { get; set; }

        /// <summary>
        /// 状态显示名称
        /// </summary>
        public string StatusDisplayName { get; set; }

        /// <summary>
        /// QR码内容
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// 小时费率
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// 台桌描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 当前会话ID（如果有）
        /// </summary>
        public Guid? CurrentSessionId { get; set; }

        /// <summary>
        /// 当前会话开始时间
        /// </summary>
        public DateTime? CurrentSessionStartTime { get; set; }

        /// <summary>
        /// 当前已用时长（分钟）
        /// </summary>
        public int CurrentDurationMinutes { get; set; }

        /// <summary>
        /// 预计费用
        /// </summary>
        public decimal EstimatedAmount { get; set; }
    }

    /// <summary>
    /// 创建台桌DTO
    /// </summary>
    public class CreateBilliardTableDto
    {
        /// <summary>
        /// 门店ID
        /// </summary>
        [Required]
        public Guid StoreId { get; set; }

        /// <summary>
        /// 台桌编号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Number { get; set; }

        /// <summary>
        /// 台桌名称
        /// </summary>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 小时费率
        /// </summary>
        [Range(0.01, 9999.99)]
        public decimal HourlyRate { get; set; } = 30m;

        /// <summary>
        /// 台桌描述
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }

    /// <summary>
    /// 更新台桌DTO
    /// </summary>
    public class UpdateBilliardTableDto
    {
        /// <summary>
        /// 台桌编号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Number { get; set; }

        /// <summary>
        /// 台桌名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 小时费率
        /// </summary>
        [Range(0.01, 9999.99)]
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// 台桌描述
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }

    /// <summary>
    /// 台桌查询参数DTO
    /// </summary>
    public class GetBilliardTablesInput : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 门店ID过滤
        /// </summary>
        public Guid? StoreId { get; set; }

        /// <summary>
        /// 状态过滤
        /// </summary>
        public TableStatus? Status { get; set; }

        /// <summary>
        /// 关键词搜索（台号或名称）
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 是否只显示启用的台桌
        /// </summary>
        public bool? IsEnabled { get; set; }
    }
}