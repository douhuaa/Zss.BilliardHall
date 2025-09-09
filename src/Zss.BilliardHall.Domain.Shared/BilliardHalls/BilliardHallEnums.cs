namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌状态枚举
/// </summary>
public enum BilliardTableStatus
{
    /// <summary>
    /// 可用
    /// </summary>
    Available = 1,
    
    /// <summary>
    /// 占用中
    /// </summary>
    Occupied = 2,
    
    /// <summary>
    /// 已预约
    /// </summary>
    Reserved = 3,
    
    /// <summary>
    /// 维护中
    /// </summary>
    Maintenance = 4,
    
    /// <summary>
    /// 故障
    /// </summary>
    OutOfOrder = 5
}

/// <summary>
/// 台球桌类型枚举
/// </summary>
public enum BilliardTableType
{
    /// <summary>
    /// 中式八球
    /// </summary>
    ChineseEightBall = 1,
    
    /// <summary>
    /// 英式斯诺克
    /// </summary>
    Snooker = 2,
    
    /// <summary>
    /// 美式九球
    /// </summary>
    AmericanNineBall = 3,
    
    /// <summary>
    /// 开伦台球
    /// </summary>
    Carom = 4
}