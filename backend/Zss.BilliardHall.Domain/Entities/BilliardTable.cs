using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace Zss.BilliardHall.Entities
{
    /// <summary>
    /// 台桌实体
    /// </summary>
    public class BilliardTable : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 门店ID
        /// </summary>
        public Guid StoreId { get; set; }

        /// <summary>
        /// 台桌编号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Number { get; set; }

        /// <summary>
        /// 台桌名称/别名
        /// </summary>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 台桌状态
        /// </summary>
        public TableStatus Status { get; set; }

        /// <summary>
        /// QR码内容
        /// </summary>
        [StringLength(500)]
        public string QrCode { get; set; }

        /// <summary>
        /// 小时费率（元/小时）
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// 台桌描述
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 关联的门店
        /// </summary>
        public virtual Store Store { get; set; }

        protected BilliardTable()
        {
        }

        public BilliardTable(Guid id, Guid storeId, string number, string name = null)
            : base(id)
        {
            StoreId = storeId;
            Number = number;
            Name = name ?? $"{number}号台";
            Status = TableStatus.Idle;
            IsEnabled = true;
            HourlyRate = 30m; // 默认费率
            QrCode = $"table_{id}";
            SortOrder = 0;
        }

        public void UpdateInfo(string number, string name, decimal hourlyRate, string description = null)
        {
            Number = number;
            Name = name;
            HourlyRate = hourlyRate;
            Description = description;
        }

        public void SetStatus(TableStatus status)
        {
            Status = status;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
            Status = TableStatus.Maintenance;
        }

        public void GenerateQrCode()
        {
            QrCode = $"table_{Id}_{DateTime.UtcNow.Ticks}";
        }

        public bool CanStartSession()
        {
            return IsEnabled && Status == TableStatus.Idle;
        }

        public bool CanLock()
        {
            return IsEnabled && Status == TableStatus.Idle;
        }
    }
}