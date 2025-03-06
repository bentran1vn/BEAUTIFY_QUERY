using BEAUTIFY_QUERY.CONTRACT.Services.OrderDetails;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.OrderDetails;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/order-details";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("OrderDetails")
            .MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("{orderId:guid}", GetOrderDetails);
    }

    private static async Task<IResult> GetOrderDetails(ISender sender,
        Guid orderId)
    {
        var result = await sender.Send(new Query.GetOrderDetailsByOrderIdQuery(orderId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}