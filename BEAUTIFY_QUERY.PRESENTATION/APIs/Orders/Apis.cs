using BEAUTIFY_QUERY.CONTRACT.Services.Orders;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Orders;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/orders";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Orders")
            .MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet(string.Empty, GetOrder).RequireAuthorization();
        gr1.MapGet("{id}", GetOrderById);
    }

    private static async Task<IResult> GetOrder(ISender sender, string? searchTerm = null, string? sortColumn = null,
        string? sortOrder = null, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetOrdersByCustomerId(searchTerm, sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder), pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetOrderById(ISender sender, string id)
    {
        var result = await sender.Send(new Query.GetOrderById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}