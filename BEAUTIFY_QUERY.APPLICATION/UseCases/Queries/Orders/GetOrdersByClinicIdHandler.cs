using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using Microsoft.EntityFrameworkCore;

/// <summary>
///api/v{version:apiVersion}/orders/clinic
/// </summary>
namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetOrdersByClinicIdHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<Order, Guid> orderRepository,
    IRepositoryBase<ClinicTransaction, Guid> clinicTransactionRepositoryBase)
    : IQueryHandler<Query.GetOrdersByClinicId, PagedResult<Response.Order>>
{
    public async Task<Result<PagedResult<Response.Order>>> Handle(Query.GetOrdersByClinicId request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim();
        
        var tranQuery = clinicTransactionRepositoryBase.FindAll(x =>
            x.ClinicId == currentUserService.ClinicId &&
            x.IsDeleted == false);

        var query = orderRepository.FindAll(x => x.IsDeleted == false);
        
        query= query.Join(
            tranQuery,
            order => order.Id,
            transaction => transaction.OrderId,
            (order, transaction) => transaction.Order!
        );

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var pattern = $"%{searchTerm}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Id.ToString(), pattern) ||
                EF.Functions.Like(x.Customer.FirstName, pattern) ||
                EF.Functions.Like(x.Customer.LastName, pattern) ||
                EF.Functions.Like(x.Service.Name, pattern) ||
                EF.Functions.Like(x.Customer.PhoneNumber, pattern) ||
                EF.Functions.Like(x.FinalAmount.ToString(), pattern) ||
                EF.Functions.Like(x.Discount.ToString(), pattern) ||
                EF.Functions.Like(x.DepositAmount.ToString(), pattern) ||
                EF.Functions.Like(x.LivestreamRoom.Name, pattern)
            );
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
        
        query = request.LiveStreamId != null
            ? query.Where(x => x.LivestreamRoomId == request.LiveStreamId)
            : query;
        
        query = request.IsLiveStream != null
            ? request.IsLiveStream == true ? query.Where(x => x.LivestreamRoomId != null) : query.Where(x => x.LivestreamRoomId == null)
            : query;

        var orders = await PagedResult<Order>.CreateAsync(query, request.PageIndex, request.PageSize);

        var mapped = orders.Items.Select(x =>
                new Response.Order(
                    x.Id,
                    x.Customer.FullName,
                    x.Service.Name,
                    x.TotalAmount,
                    x.Discount,
                    x.DepositAmount,
                    x.FinalAmount,
                    x.CreatedOnUtc,
                    x.Status,
                    x.Customer.PhoneNumber,
                    x.Customer.Email,
                    x.LivestreamRoomId != null,
                    x.LivestreamRoomId != null ? x.LivestreamRoom?.Name : null))
            .ToList();

        return Result.Success(
            new PagedResult<Response.Order>(mapped, orders.PageIndex, orders.PageSize, orders.TotalCount));
    }

    private static Expression<Func<Order, object>> GetSortProperty(Query.GetOrdersByClinicId request)
    {
        return request.SortColumn switch
        {
            "customerName" => x => x.Customer.FullName,
            "serviceName" => x => x.Service.Name,
            "totalAmount" => x => x.TotalAmount,
            "discount" => x => x.Discount,
            "depositAmount" => x => x.DepositAmount,
            "finalAmount" => x => x.FinalAmount,
            "oderDate" => x => x.OrderDate,
            _ => x => x.CreatedOnUtc,
        };
    }
}