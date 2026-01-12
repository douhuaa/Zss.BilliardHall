using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.Modules.Members;

internal static class MemberErrorCodes
{
    public static readonly ErrorCode InvalidTopUpAmount =
        new("Member.InvalidTopUpAmount");

    public static readonly ErrorCode InvalidDeductAmount =
        new("Member.InvalidDeductAmount");

    public static readonly ErrorCode InsufficientBalance =
        new("Member.InsufficientBalance");

    public static readonly ErrorCode InvalidAwardPoints =
        new("Member.InvalidAwardPoints");
}
