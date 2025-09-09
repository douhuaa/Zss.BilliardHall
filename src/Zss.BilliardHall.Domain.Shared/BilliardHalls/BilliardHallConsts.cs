namespace Zss.BilliardHall.BilliardHalls;

public static class BilliardHallConsts
{
    public const int MaxNameLength = 100;
    public const int MaxAddressLength = 500;
    public const int MaxDescriptionLength = 1000;
    
    public const int MaxPhoneLength = 20;
    public const int MaxEmailLength = 100;
}

public static class BilliardTableConsts
{
    public const int MaxDescriptionLength = 500;
    public const decimal MinHourlyRate = 0.01m;
    public const decimal MaxHourlyRate = 9999.99m;
}

public static class CustomerConsts
{
    public const int MaxNameLength = 100;
    public const int MaxPhoneLength = 20;
    public const int MaxEmailLength = 100;
    public const int MaxNotesLength = 1000;
}

public static class ReservationConsts
{
    public const int MinDurationMinutes = 30;
    public const int MaxDurationMinutes = 480; // 8 hours
    public const int MaxNotesLength = 500;
}