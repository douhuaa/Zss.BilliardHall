using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.Modules.Sessions;

/// <summary>
/// Sessions 模块错误描述符（示例）
/// Sessions module error descriptors (example)
/// </summary>
internal static class SessionErrorDescriptors
{
    private const string ModuleName = "Sessions";

    // Helper method for consistent error code formatting
    private static string ErrorCode(string category, string specific) => 
        $"{ModuleName}:{category}.{specific}";

    // NotFound errors
    public static ErrorDescriptor SessionNotFound(Guid sessionId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("NotFound", "Session"))
            .WithCategory(ErrorCategory.NotFound)
            .WithMessage("会话不存在: {SessionId}")
            .AddContext("SessionId", sessionId)
            .Build();

    // Validation errors
    public static ErrorDescriptor InvalidDuration(TimeSpan duration) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Validation", "InvalidDuration"))
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("会话时长无效，必须大于 0，实际: {Duration}")
            .AddContext("Duration", duration)
            .Build();

    public static ErrorDescriptor InvalidStartTime(DateTimeOffset startTime) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Validation", "InvalidStartTime"))
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("开始时间无效: {StartTime}")
            .AddContext("StartTime", startTime)
            .Build();

    // Business rule errors
    public static ErrorDescriptor SessionAlreadyEnded(Guid sessionId, DateTimeOffset endedAt) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Business", "SessionAlreadyEnded"))
            .WithCategory(ErrorCategory.Business)
            .WithMessage("会话已结束: {SessionId}, 结束时间: {EndedAt}")
            .AddContext("SessionId", sessionId)
            .AddContext("EndedAt", endedAt)
            .Build();

    public static ErrorDescriptor SessionNotStarted(Guid sessionId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Business", "SessionNotStarted"))
            .WithCategory(ErrorCategory.Business)
            .WithMessage("会话尚未开始: {SessionId}")
            .AddContext("SessionId", sessionId)
            .Build();

    public static ErrorDescriptor CannotPauseEndedSession(Guid sessionId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Business", "CannotPauseEndedSession"))
            .WithCategory(ErrorCategory.Business)
            .WithMessage("无法暂停已结束的会话: {SessionId}")
            .AddContext("SessionId", sessionId)
            .Build();

    // Conflict errors
    public static ErrorDescriptor ActiveSessionExists(Guid tableId, Guid existingSessionId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("Conflict", "ActiveSessionExists"))
            .WithCategory(ErrorCategory.Conflict)
            .WithMessage("台球桌已有活动会话: {TableId}, 会话: {ExistingSessionId}")
            .AddContext("TableId", tableId)
            .AddContext("ExistingSessionId", existingSessionId)
            .Build();

    // InvalidStatus errors
    public static ErrorDescriptor InvalidSessionStatus(Guid sessionId, string currentStatus, string expectedStatus) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode(ErrorCode("InvalidStatus", "Session"))
            .WithCategory(ErrorCategory.InvalidStatus)
            .WithMessage("会话状态不符，当前: {CurrentStatus}, 期望: {ExpectedStatus}")
            .AddContext("SessionId", sessionId)
            .AddContext("CurrentStatus", currentStatus)
            .AddContext("ExpectedStatus", expectedStatus)
            .Build();
}

/// <summary>
/// Sessions 模块领域异常
/// Sessions module domain exception
/// </summary>
public sealed class SessionsDomainException : ModuleDomainException
{
    public SessionsDomainException(ErrorDescriptor errorDescriptor)
        : base(errorDescriptor)
    {
    }

    public SessionsDomainException(ErrorDescriptor errorDescriptor, Exception innerException)
        : base(errorDescriptor, innerException)
    {
    }
}
