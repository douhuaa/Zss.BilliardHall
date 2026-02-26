using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 错误码描述符，承载错误码的完整语义（HTTP 状态、标题、日志级别）
/// </summary>
public sealed record ErrorDescriptor(
    string ErrorCode,
    int HttpStatusCode,
    string Title,
    LogLevel LogLevel = LogLevel.Error);
