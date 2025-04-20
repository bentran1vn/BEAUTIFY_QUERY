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
    IRepositoryBase<Order, Guid> orderRepository)
    : IQueryHandler<Query.GetOrdersByClinicId, PagedResult<Response.Order>>
{
    public async Task<Result<PagedResult<Response.Order>>> Handle(Query.GetOrdersByClinicId request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim();

        var query = orderRepository.FindAll(x =>
            x.Service.ClinicServices.FirstOrDefault().ClinicId == currentUserService.ClinicId);

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
                    x.LivestreamRoomId != null ? x.LivestreamRoom.Name : null))
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