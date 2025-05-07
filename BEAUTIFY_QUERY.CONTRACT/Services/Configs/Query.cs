using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Configs;
public static class Query
{
    public record GetConfigs(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetConfigsResponse>>;

    public record GetConfigById(Guid Id) : IQuery<Response.GetConfigByIdResponse>;
}