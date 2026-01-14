using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Tables;

public static class TableErrorCodes
{
    public static readonly ErrorCode NotFound =
        new("Tables.NotFound", "台球桌不存在");

    public static readonly ErrorCode Unavailable =
        new("Tables.Unavailable", "台球桌不可用");

    public static readonly ErrorCode AlreadyReserved =
        new("Tables.AlreadyReserved", "台球桌已被预订");

    public static readonly ErrorCode InvalidStatus =
        new("Tables.InvalidStatus", "台球桌状态无效");
}
