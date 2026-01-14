using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Sessions;

/// <summary>
/// 打球时段模块领域异常工厂
/// Factory for creating domain exceptions in Sessions module
/// </summary>
/// <remarks>
/// 提供统一的异常创建入口，确保异常消息格式一致且支持本地化
/// Provides unified exception creation to ensure consistent error message format and i18n support
/// </remarks>
public static class SessionsDomainErrors
{
    public static DomainException NotFound(Guid sessionId)
    {
        return new DomainException(
            SessionErrorCodes.NotFound.Code,
            $"打球时段不存在: {sessionId}"
        );
    }

    public static DomainException AlreadyEnded(Guid sessionId)
    {
        return new DomainException(
            SessionErrorCodes.AlreadyEnded.Code,
            $"打球时段 {sessionId} 已结束"
        );
    }

    public static DomainException AlreadyStarted(Guid sessionId)
    {
        return new DomainException(
            SessionErrorCodes.AlreadyStarted.Code,
            $"打球时段 {sessionId} 已开始"
        );
    }

    public static DomainException InvalidStatus(Guid sessionId, string currentStatus, string operation)
    {
        return new DomainException(
            SessionErrorCodes.InvalidStatus.Code,
            $"打球时段 {sessionId} 状态无效: 当前状态 {currentStatus}，无法执行操作 {operation}"
        );
    }

    public static DomainException InvalidPauseStatus(Guid sessionId, string currentStatus)
    {
        return new DomainException(
            SessionErrorCodes.InvalidPauseStatus.Code,
            $"无法暂停打球时段 {sessionId}: 当前状态 {currentStatus}"
        );
    }

    public static DomainException InvalidResumeStatus(Guid sessionId, string currentStatus)
    {
        return new DomainException(
            SessionErrorCodes.InvalidResumeStatus.Code,
            $"无法恢复打球时段 {sessionId}: 当前状态 {currentStatus}"
        );
    }
}
