using friasco_api.Enums;

namespace friasco_api.Models;

public class Trip
{
    private Guid _id;
    private Guid _userId;
    private string _location;
    private DateTime _startDate;
    private DateTime _endDate;
    private TripStatusEnum _status;
    private TripPrivacyEnum _privacyStatus;

    public Trip(
        Guid id,
        Guid userId,
        string location,
        DateTime startDate,
        DateTime endDate,
        TripStatusEnum status,
        TripPrivacyEnum privacyStatus
        )
    {
        _id = id;
        _userId = userId;
        _location = location;
        _startDate = startDate;
        _endDate = endDate;
        _status = status;
        _privacyStatus = privacyStatus;
    }
}
