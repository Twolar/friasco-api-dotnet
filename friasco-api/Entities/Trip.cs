using friasco_api.Enums;

namespace friasco_api.Entities;

public class Trip
{
    public Guid Id;
    public Guid UserId;
    public string? Location;
    public DateTime? StartDate;
    public DateTime? EndDate;
    public TripStatusEnum Status;
    public TripPrivacyEnum PrivacyStatus;
}
