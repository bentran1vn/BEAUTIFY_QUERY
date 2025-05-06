using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Orders;
public static class Query
{
    public record GetOrdersByCustomerId(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.Order>>;


    //Todo Check roles
    public record GetOrdersByClinicId(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize,
        Guid? LiveStreamId,
        bool? IsLiveStream) : IQuery<PagedResult<Response.Order>>;


    public record GetOrderById(Guid Id) : IQuery<Response.OrderById>;

    public record GetClinicOrderBranchesQuery(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.Order>>;
}