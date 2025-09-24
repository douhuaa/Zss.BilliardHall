namespace Zss.BilliardHall.Shared;

/// <summary>
/// 台球桌状态枚举
/// </summary>
public enum TableStatus
{
    /// <summary>
    /// 空闲
    /// </summary>
    Idle = 0,

    /// <summary>
    /// 使用中
    /// </summary>
    InUse = 1,

    /// <summary>
    /// 已预约
    /// </summary>
    Reserved = 2,

    /// <summary>
    /// 维护中
    /// </summary>
    Maintenance = 3,

    /// <summary>
    /// 故障
    /// </summary>
    Error = 4
}

/// <summary>
/// 会话状态枚举
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// 进行中
    /// </summary>
    Active = 0,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 1,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// 待结算
    /// </summary>
    PendingSettlement = 3
}

/// <summary>
/// 支付状态枚举
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// 待支付
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 支付成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 支付失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 已退款
    /// </summary>
    Refunded = 3
}