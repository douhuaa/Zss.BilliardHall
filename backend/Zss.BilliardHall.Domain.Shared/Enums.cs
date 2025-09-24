namespace Zss.BilliardHall
{
    /// <summary>
    /// 门店状态枚举
    /// </summary>
    public enum StoreStatus
    {
        /// <summary>
        /// 营业中
        /// </summary>
        Active = 1,
        
        /// <summary>
        /// 暂停营业
        /// </summary>
        Suspended = 2,
        
        /// <summary>
        /// 永久关闭
        /// </summary>
        Closed = 3
    }

    /// <summary>
    /// 台桌状态枚举
    /// </summary>
    public enum TableStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 1,
        
        /// <summary>
        /// 已锁定（预约）
        /// </summary>
        Locked = 2,
        
        /// <summary>
        /// 使用中
        /// </summary>
        InUse = 3,
        
        /// <summary>
        /// 待结算
        /// </summary>
        PendingClose = 4,
        
        /// <summary>
        /// 已结束
        /// </summary>
        Closed = 5,
        
        /// <summary>
        /// 维护中
        /// </summary>
        Maintenance = 6,
        
        /// <summary>
        /// 异常状态
        /// </summary>
        Error = 7
    }

    /// <summary>
    /// 会话状态枚举
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        Active = 1,
        
        /// <summary>
        /// 计费冻结（等待支付）
        /// </summary>
        Frozen = 2,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 3,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// 支付状态枚举
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// 支付成功
        /// </summary>
        Paid = 2,
        
        /// <summary>
        /// 支付失败
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 4,
        
        /// <summary>
        /// 已退款
        /// </summary>
        Refunded = 5
    }

    /// <summary>
    /// 支付方式枚举
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// 微信支付
        /// </summary>
        WeChat = 1,
        
        /// <summary>
        /// 支付宝
        /// </summary>
        Alipay = 2,
        
        /// <summary>
        /// 现金
        /// </summary>
        Cash = 3,
        
        /// <summary>
        /// 会员余额
        /// </summary>
        MemberBalance = 4
    }

    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// 在线
        /// </summary>
        Online = 1,
        
        /// <summary>
        /// 离线
        /// </summary>
        Offline = 2,
        
        /// <summary>
        /// 故障
        /// </summary>
        Fault = 3,
        
        /// <summary>
        /// 维护
        /// </summary>
        Maintenance = 4
    }
}