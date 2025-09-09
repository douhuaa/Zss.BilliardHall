namespace Zss.BilliardHall;

public static class BilliardHallDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    
    // Billiard Hall Error Codes
    public const string BilliardHallNameAlreadyExists = "BilliardHall:BilliardHallNameAlreadyExists";
    
    // Billiard Table Error Codes
    public const string BilliardTableNumberAlreadyExists = "BilliardHall:BilliardTableNumberAlreadyExists";
    public const string InvalidHourlyRate = "BilliardHall:InvalidHourlyRate";
    public const string CannotChangeStatusFromOutOfOrder = "BilliardHall:CannotChangeStatusFromOutOfOrder";
    public const string TableNotAvailable = "BilliardHall:TableNotAvailable";
    
    // Customer Error Codes
    public const string CustomerPhoneAlreadyExists = "BilliardHall:CustomerPhoneAlreadyExists";
    public const string CustomerEmailAlreadyExists = "BilliardHall:CustomerEmailAlreadyExists";
    
    // Reservation Error Codes
    public const string TimeSlotConflict = "BilliardHall:TimeSlotConflict";
    public const string InvalidReservationTime = "BilliardHall:InvalidReservationTime";
    public const string ReservationInPast = "BilliardHall:ReservationInPast";
    public const string ReservationTooFar = "BilliardHall:ReservationTooFar";
    public const string CannotCancelReservation = "BilliardHall:CannotCancelReservation";
}
