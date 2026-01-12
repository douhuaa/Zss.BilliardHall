namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 结构化错误码常量
/// </summary>
/// <remarks>
/// 格式：{Area}:{Key}
/// 用途：
/// 1. 前端错误识别与差异化处理
/// 2. 日志聚合与统计分析
/// 3. 重试策略配置
/// 4. 多语言支持
/// </remarks>
public static class ErrorCodes
{
    /// <summary>
    /// 台球桌模块错误码
    /// </summary>
    public static class Tables
    {
        public const string NotFound = "Tables:NotFound";
        public const string Unavailable = "Tables:Unavailable";
        public const string AlreadyReserved = "Tables:AlreadyReserved";
        public const string InvalidStatus = "Tables:InvalidStatus";
    }

    /// <summary>
    /// 打球时段模块错误码
    /// </summary>
    public static class Sessions
    {
        public const string NotFound = "Sessions:NotFound";
        public const string AlreadyEnded = "Sessions:AlreadyEnded";
        public const string AlreadyStarted = "Sessions:AlreadyStarted";
        public const string InvalidStatus = "Sessions:InvalidStatus";
        public const string InvalidPauseStatus = "Sessions:InvalidPauseStatus";
        public const string InvalidResumeStatus = "Sessions:InvalidResumeStatus";
    }

    /// <summary>
    /// 计费模块错误码
    /// </summary>
    public static class Billing
    {
        public const string InsufficientBalance = "Billing:InsufficientBalance";
        public const string InvalidAmount = "Billing:InvalidAmount";
        public const string CalculationFailed = "Billing:CalculationFailed";
        public const string TableUnavailable = "Billing:TableUnavailable";
    }

    /// <summary>
    /// 支付模块错误码
    /// </summary>
    public static class Payments
    {
        public const string NotFound = "Payments:NotFound";
        public const string InvalidStatus = "Payments:InvalidStatus";
        public const string PaymentFailed = "Payments:PaymentFailed";
        public const string RefundNotAllowed = "Payments:RefundNotAllowed";
        public const string AmountMismatch = "Payments:AmountMismatch";
    }

    /// <summary>
    /// 会员模块错误码
    /// </summary>
    public static class Members
    {
        public const string NotFound = "Members:NotFound";
        public const string AlreadyExists = "Members:AlreadyExists";
        public const string InvalidTier = "Members:InvalidTier";
        public const string InsufficientBalance = "Members:InsufficientBalance";
        public const string InvalidPhone = "Members:InvalidPhone";
    }

    /// <summary>
    /// 设备模块错误码
    /// </summary>
    public static class Devices
    {
        public const string NotFound = "Devices:NotFound";
        public const string Offline = "Devices:Offline";
        public const string CommandFailed = "Devices:CommandFailed";
        public const string InvalidCommand = "Devices:InvalidCommand";
    }

    /// <summary>
    /// 通用错误码
    /// </summary>
    public static class Common
    {
        public const string ValidationFailed = "Common:ValidationFailed";
        public const string Unauthorized = "Common:Unauthorized";
        public const string Forbidden = "Common:Forbidden";
        public const string ConcurrencyConflict = "Common:ConcurrencyConflict";
        public const string InternalError = "Common:InternalError";
    }
}
