using Zss.BilliardHall.BuildingBlocks.Exceptions;

namespace Zss.BilliardHall.Modules.Sessions;

public static class SessionErrorCodes
{
    public static readonly ErrorCode NotFound =
        new("Sessions.NotFound", "打球时段不存在");

    public static readonly ErrorCode AlreadyEnded =
        new("Sessions.AlreadyEnded", "打球时段已结束");

    public static readonly ErrorCode AlreadyStarted =
        new("Sessions.AlreadyStarted", "打球时段已开始");

    public static readonly ErrorCode InvalidStatus =
        new("Sessions.InvalidStatus", "打球时段状态无效");

    public static readonly ErrorCode InvalidPauseStatus =
        new("Sessions.InvalidPauseStatus", "无法暂停该打球时段");

    public static readonly ErrorCode InvalidResumeStatus =
        new("Sessions.InvalidResumeStatus", "无法恢复该打球时段");
}
