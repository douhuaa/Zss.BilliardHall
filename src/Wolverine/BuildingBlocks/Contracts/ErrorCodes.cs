namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 通用技术性错误码常量
/// Common technical error codes
/// </summary>
/// <remarks>
/// 格式：{Area}:{Key}
/// 
/// ⚠️ 重要约束 (Important Constraints):
/// 此文件只应包含"技术性失败类型"，不应包含"业务规则"
/// This file should only contain "technical failure types", not "business rules"
/// 
/// ✅ 允许 (Allowed): ValidationFailed, Unauthorized, NotFound (技术性)
/// ❌ 禁止 (Forbidden): InsufficientBalance, CannotReserveAtNight (业务规则)
/// 
/// 模块特定错误码应定义在各自模块内：
/// Module-specific error codes should be defined within their respective modules:
/// - Modules/Members/MemberErrorCodes.cs
/// - Modules/Tables/TableErrorCodes.cs
/// - Modules/Sessions/SessionErrorCodes.cs
/// 
/// 用途：
/// 1. 前端错误识别与差异化处理
/// 2. 日志聚合与统计分析
/// 3. 重试策略配置
/// 4. 多语言支持
/// </remarks>
public static class ErrorCodes
{
    /// <summary>
    /// 通用技术性错误码
    /// Common technical error codes
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
