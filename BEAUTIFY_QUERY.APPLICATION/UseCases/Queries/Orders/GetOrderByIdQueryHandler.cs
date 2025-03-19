using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetOrderByIdQueryHandler(IRepositoryBase<Order, Guid> orderRepositoryBase)
    : IQueryHandler<Query.GetOrderById,
        Response.Order>
{
    public async Task<Result<Response.Order>> Handle(Query.GetOrderById request, CancellationToken cancellationToken)
    {
        var order = await orderRepositoryBase.FindSingleAsync(x => x.Id.ToString().Contains(request.Id),
            cancellationToken);
        if (order == null)
            return Result.Failure<Response.Order>(new Error("404", "Order Not Found"));

        return Result.Success(new Response.Order(
            order.Id,
            order.Customer.FullName,
            order.Service.Name,
            order.TotalAmount,
            order.Discount,
            order.FinalAmount,
            order.OrderDate,
            order.Status));
    }
}