using BEAUTIFY_QUERY.CONTRACT.Services.OrderDetails;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.OrderDetails;
internal sealed class GetOrderDetailsByOrderIdQueryHandler(IRepositoryBase<Order, Guid> orderRepositoryBase)
    : IQueryHandler<Query.GetOrderDetailsByOrderIdQuery, List<Response.OrderDetailResponse>>
{
    public async Task<Result<List<Response.OrderDetailResponse>>> Handle(
        Query.GetOrderDetailsByOrderIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepositoryBase.FindByIdAsync(request.OrderId, cancellationToken, x => x.OrderDetails,
            x => x.Customer);
        if (order == null)
            return Result.Failure<List<Response.OrderDetailResponse>>(new Error("404", "Order not found"));
        var orderDetails = order.OrderDetails.Select(od => new Response.OrderDetailResponse
        {
            Id = od.Id,
            ServiceName = od.Order.Service.Name,
            ProcedurePriceType = od.ProcedurePriceType.Name,
            Price = od.Price,
            Duration = od.ProcedurePriceType.Duration,
            CustomerEmail = order.Customer?.Email,
            CustomerPhone = order.Customer?.PhoneNumber
        }).ToList();
        return Result.Success(orderDetails);
    }
}