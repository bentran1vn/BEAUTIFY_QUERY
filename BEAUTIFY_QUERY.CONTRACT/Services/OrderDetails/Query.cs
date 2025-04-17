namespace BEAUTIFY_QUERY.CONTRACT.Services.OrderDetails;
public static class Query
{
    public record GetOrderDetailsByOrderIdQuery(Guid OrderId) : IQuery<List<Response.OrderDetailResponse>>;
}