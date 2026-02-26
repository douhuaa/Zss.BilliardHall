namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 平台层统一错误消息常量
/// </summary>
public static class PlatformErrorMessages
{
    /// <summary>
    /// 验证失败时 PlatformValidationException 的默认 message。
    /// 具体字段错误通过 Errors 字典传递，此 message 仅作摘要。
    /// </summary>
    public const string ValidationFailure = "验证失败，请检查输入数据。";
}
