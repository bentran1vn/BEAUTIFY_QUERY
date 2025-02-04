namespace BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
public static class Response
{
    public class GetSubscriptionResponse()
    {
        public string? DocumentId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public int Duration { get; set; }
        public bool IsActivated { get; set; }
    }
}