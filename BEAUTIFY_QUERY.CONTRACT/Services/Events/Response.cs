namespace BEAUTIFY_QUERY.CONTRACT.Services.Events;

public class Response
{
    public class EventResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string ImageUrl { get; set; }
        public string ClinicName { get; set; }
        public Guid ClinicId { get; set; }
        public EventDetailResponse EventDetail { get; set; }
    }
    
    public class EventDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string ImageUrl { get; set; }
        public string ClinicName { get; set; }
        public Guid ClinicId { get; set; }
        public List<LivestreamRoomResponse> LivestreamRooms { get; set; }
    }
    
    public class  LivestreamRoomResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public TimeOnly? StartDate { get; set; }
        public TimeOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public DateOnly? Date { get; set; }
        public string? Type { get; set; }
        public LiveStreamDetail? LiveStreamDetail { get; set; }
    }

    public class LiveStreamDetail
    {
        public int JoinCount { get; set; }
        public int MessageCount { get; set; }
        public int ReactionCount { get; set; }
        public int TotalActivities { get; set; }
        public int TotalBooking { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
    }
}