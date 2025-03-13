using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
public static class Query
{
    public record GetSubscription(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetSubscriptionResponse>>;

    public record GetSubscriptionById(Guid Id) : IQuery<Response.GetSubscriptionResponse>;
}