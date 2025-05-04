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
        public Guid? LiveStreamRoomId { get; set; }
    }
}