namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 平台层公共错误码常量
/// </summary>
public static class CommonErrorCodes
{
    /// <summary>输入验证失败（FluentValidation 或手动验证）</summary>
    public const string ValidationFailed = "COMMON_VALIDATION_FAILED";

    /// <summary>未知/未注册错误（fallback）</summary>
    public const string UnknownError = "COMMON_UNKNOWN_ERROR";
}
