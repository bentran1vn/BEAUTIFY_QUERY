using BEAUTIFY_QUERY.CONTRACT.Services.Surveys;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Surveys;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/surveys";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Surveys").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetSurvey);
    }

    private static async Task<IResult> GetSurvey(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetSurvey(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}