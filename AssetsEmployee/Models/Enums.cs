namespace AssetsEmployee.Models
{
    public enum AssetStatus
    {
        Available = 1,
        Assigned = 2,
        UnderRepair = 3,
        Retired = 4
    }

    public enum RequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Fulfilled = 4
    }
}