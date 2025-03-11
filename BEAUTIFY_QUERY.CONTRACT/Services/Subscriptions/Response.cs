namespace BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
public static class Response
{
    public class GetSubscriptionResponse()
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int Duration { get; set; }
        public bool IsActivated { get; set; }
        public int LimitBranch { get; set; }
        public int LimitLiveStream { get; set; }
        public int EnhancedViewer { get; set; }
    }
}