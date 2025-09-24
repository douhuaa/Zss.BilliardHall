using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace Zss.BilliardHall.Entities
{
    /// <summary>
    /// 门店实体
    /// </summary>
    public class Store : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 门店名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 门店地址
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(20)]
        public string Phone { get; set; }

        /// <summary>
        /// 门店状态
        /// </summary>
        public StoreStatus Status { get; set; }

        /// <summary>
        /// 营业时间开始
        /// </summary>
        public TimeSpan OpenTime { get; set; }

        /// <summary>
        /// 营业时间结束
        /// </summary>
        public TimeSpan CloseTime { get; set; }

        /// <summary>
        /// 基础费率（元/小时）
        /// </summary>
        public decimal BaseHourlyRate { get; set; }

        protected Store()
        {
        }

        public Store(Guid id, string name, string address = null, string phone = null)
            : base(id)
        {
            Name = name;
            Address = address;
            Phone = phone;
            Status = StoreStatus.Active;
            OpenTime = new TimeSpan(9, 0, 0); // 默认9:00开门
            CloseTime = new TimeSpan(23, 0, 0); // 默认23:00关门
            BaseHourlyRate = 30m; // 默认30元/小时
        }

        public void UpdateInfo(string name, string address, string phone)
        {
            Name = name;
            Address = address;
            Phone = phone;
        }

        public void SetBusinessHours(TimeSpan openTime, TimeSpan closeTime)
        {
            if (openTime >= closeTime)
            {
                throw new ArgumentException("开门时间必须早于关门时间");
            }

            OpenTime = openTime;
            CloseTime = closeTime;
        }

        public void SetStatus(StoreStatus status)
        {
            Status = status;
        }
    }
}