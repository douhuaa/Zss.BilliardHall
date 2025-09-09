namespace Zss.BilliardHall.Permissions;

public static class BilliardHallPermissions
{
    public const string GroupName = "BilliardHall";

    public static class BilliardHalls
    {
        public const string Default = GroupName + ".BilliardHalls";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageSettings = Default + ".ManageSettings";
    }
    
    public static class BilliardTables
    {
        public const string Default = GroupName + ".BilliardTables";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ChangeStatus = Default + ".ChangeStatus";
        public const string ViewReservations = Default + ".ViewReservations";
    }
    
    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewDetails = Default + ".ViewDetails";
        public const string ManageBalance = Default + ".ManageBalance";
        public const string ManagePoints = Default + ".ManagePoints";
    }
    
    public static class Reservations
    {
        public const string Default = GroupName + ".Reservations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Cancel = Default + ".Cancel";
        public const string Confirm = Default + ".Confirm";
        public const string StartUsing = Default + ".StartUsing";
        public const string Complete = Default + ".Complete";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
