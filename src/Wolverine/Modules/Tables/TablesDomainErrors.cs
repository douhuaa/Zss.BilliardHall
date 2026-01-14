using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Tables;

/// <summary>
/// 台球桌模块领域异常工厂
/// Factory for creating domain exceptions in Tables module
/// </summary>
/// <remarks>
/// 提供统一的异常创建入口，确保异常消息格式一致且支持本地化
/// Provides unified exception creation to ensure consistent error message format and i18n support
/// </remarks>
public static class TablesDomainErrors
{
    public static DomainException NotFound(Guid tableId)
    {
        return new DomainException(
            TableErrorCodes.NotFound.Code,
            $"台球桌不存在: {tableId}"
        );
    }

    public static DomainException Unavailable(Guid tableId, string reason)
    {
        return new DomainException(
            TableErrorCodes.Unavailable.Code,
            $"台球桌 {tableId} 不可用: {reason}"
        );
    }

    public static DomainException AlreadyReserved(Guid tableId)
    {
        return new DomainException(
            TableErrorCodes.AlreadyReserved.Code,
            $"台球桌 {tableId} 已被预订"
        );
    }

    public static DomainException InvalidStatus(Guid tableId, string currentStatus, string expectedStatus)
    {
        return new DomainException(
            TableErrorCodes.InvalidStatus.Code,
            $"台球桌 {tableId} 状态无效: 当前状态 {currentStatus}，期望状态 {expectedStatus}"
        );
    }
}
