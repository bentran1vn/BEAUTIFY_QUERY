using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
public sealed class GetOrdersByCustomerIdQueryHandler(
    IRepositoryBase<Order, Guid> orderRepositoryBase,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetOrdersByCustomerId, PagedResult<Response.Order>>
{
    public async Task<Result<PagedResult<Response.Order>>> Handle(Query.GetOrdersByCustomerId request,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(request);
        var orders = await PagedResult<Order>.CreateAsync(query, request.PageIndex, request.PageSize);
        var result = MapToResponse(orders);
        return Result.Success(result);
    }

    private IQueryable<Order> BuildQuery(Query.GetOrdersByCustomerId request)
    {
        var query = orderRepositoryBase.FindAll(x => x.CustomerId == currentUserService.UserId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) ApplySearchFilter(ref query, request.SearchTerm.Trim());

        query = query.Include(x => x.Service).Include(x => x.LivestreamRoom);
        query = ApplySorting(query, request);

        return query;
    }

    private static void ApplySearchFilter(ref IQueryable<Order> query, string searchTerm)
    {
        var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return;

        var part1 = parts[0].Trim();
        var part2 = parts[1].Trim();

        if (DateOnly.TryParse(part1, out var dateFrom) && DateOnly.TryParse(part2, out var dateTo))
            query = query.Where(x => x.OrderDate >= dateFrom && x.OrderDate <= dateTo);
        else if (decimal.TryParse(part1, out var priceFrom) && decimal.TryParse(part2, out var priceTo))
            query = query.Where(x => (x.FinalAmount >= priceFrom && x.FinalAmount <= priceTo) ||
                                     (x.Discount >= priceFrom && x.Discount <= priceTo) ||
                                     (x.TotalAmount >= priceFrom && x.TotalAmount <= priceTo));
    }

    private static IQueryable<Order> ApplySorting(IQueryable<Order> query, Query.GetOrdersByCustomerId request)
    {
        return request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
    }

    private static PagedResult<Response.Order> MapToResponse(PagedResult<Order> orders)
    {
        var mapped = orders.Items.Select(x => new Response.Order(
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
                x.LivestreamRoomId != null ? x.LivestreamRoom.Name : null
            ))
            .OrderBy(x => x.Status)
            .ToList();

        return new PagedResult<Response.Order>(mapped, orders.PageIndex, orders.PageSize, orders.TotalCount);
    }

    private static Expression<Func<Order, object>> GetSortProperty(Query.GetOrdersByCustomerId request)
    {
        return request.SortColumn switch
        {
            "totalAmount" => projection => projection.TotalAmount,
            "discount" => projection => projection.Discount,
            "finalAmount" => projection => projection.FinalAmount,
            "orderDate" => projection => projection.OrderDate,
            _ => projection => projection.CreatedOnUtc
        };
    }
}