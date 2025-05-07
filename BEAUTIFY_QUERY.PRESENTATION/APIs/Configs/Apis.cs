using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Configs;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Configs;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{verison:apiVersion}/configs";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Configs").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetConfigs).RequireAuthorization(Constant.Role.SYSTEM_ADMIN);
        gr1.MapGet("{id:guid}", GetConfigById).RequireAuthorization(Constant.Role.SYSTEM_ADMIN);
    }

    private static async Task<IResult> GetConfigs(
        ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetConfigs(searchTerm, sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetConfigById(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetConfigById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}