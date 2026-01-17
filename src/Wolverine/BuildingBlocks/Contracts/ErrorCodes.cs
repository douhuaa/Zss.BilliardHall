namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 通用技术性错误码（仅表达"故障类型"，不表达"业务决策原因"）
/// </summary>
/// <remarks>
/// ⚠️ 陷阱：ErrorCodes 只表达失败类型（NotFound/InvalidStatus），不表达业务决策（CannotReserveAtNight）
/// 业务决策相关错误码必须在模块内定义（如 MembersDomainErrors），不放入 BuildingBlocks
/// 
/// 格式：{Category}
/// 用途：
/// 1. 前端错误识别与差异化处理
/// 2. HTTP 状态码映射
/// 3. 日志聚合与统计分析
/// </remarks>
public static class ErrorCodes
{
    /// <summary>
    /// 通用错误类别
    /// </summary>
    public static class Common
    {
        public const string NotFound = "NotFound";
        public const string InvalidStatus = "InvalidStatus";
        public const string Conflict = "Conflict";
        public const string Forbidden = "Forbidden";
        public const string ValidationFailed = "ValidationFailed";
        public const string Unauthorized = "Unauthorized";
        public const string InternalError = "InternalError";
    }
}
