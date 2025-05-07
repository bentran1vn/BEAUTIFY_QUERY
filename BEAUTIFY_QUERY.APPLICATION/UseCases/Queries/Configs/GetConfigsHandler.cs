using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.Configs;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Configs;
internal sealed class GetConfigsHandler(IRepositoryBase<Config, Guid> repositoryBase)
    : IQueryHandler<Query.GetConfigs, PagedResult<Response.GetConfigsResponse>>
{
    public async Task<Result<PagedResult<Response.GetConfigsResponse>>> Handle(Query.GetConfigs request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim().ToLower();
        var query = repositoryBase.FindAll(x => !x.IsDeleted);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Where(x =>
                x.Key.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                x.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
        var total = await PagedResult<Config>.CreateAsync(query,
            request.PageIndex,
            request.PageSize);
        var mapped = total.Items.Select(x => new Response.GetConfigsResponse
        (x.Id,
            x.Key,
            x.Value)).ToList();
        return Result.Success(new PagedResult<Response.GetConfigsResponse>(mapped,
            request.PageIndex,
            request.PageSize,
            total.TotalCount
        ));
    }

    private static Expression<Func<Config, object>> GetSortProperty(
        Query.GetConfigs request)
    {
        return request.SortColumn switch
        {
            "key" => x => x.Key,
            "value" => x => x.Value,
            _ => x => x.CreatedOnUtc
        };
    }
}