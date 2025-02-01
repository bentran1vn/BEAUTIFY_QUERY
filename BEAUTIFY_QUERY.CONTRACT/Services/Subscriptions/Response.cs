namespace BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
public static class Response
{
    public record GetSubscriptionResponse(Guid DocumentId, string? Name, string? Description, decimal Price, int Duration, bool IsActivated, bool IsDeleted);
}