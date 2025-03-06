using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetOrdersByCustomerIdQueryHandler(
    IRepositoryBase<Order, Guid> orderRepositoryBase,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetOrdersByCustomerId, PagedResult<Response.Order>>
{
    public async Task<Result<PagedResult<Response.Order>>> Handle(Query.GetOrdersByCustomerId request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        var query = orderRepositoryBase.FindAll(x => x.CustomerId == currentUserService.UserId);
        if (string.IsNullOrEmpty(searchTerm))
        {
            var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var part1 = parts[0].Trim();
                var part2 = parts[1].Trim();

                if (DateTimeOffset.TryParse(part1, out var dateFrom) &&
                    DateTimeOffset.TryParse(part2, out var dateTo))
                {
                    query = query.Where(x => x.OrderDate >= dateFrom && x.OrderDate <= dateTo);
                }
                else if (decimal.TryParse(part1, out var priceFrom) &&
                         decimal.TryParse(part2, out var priceTo))
                {
                    query = query.Where(x =>
                        x.FinalAmount >= priceFrom && x.FinalAmount <= priceTo ||
                        x.Discount >= priceFrom && x.Discount <= priceTo ||
                        x.TotalAmount >= priceFrom && x.TotalAmount <= priceTo);
                }
            }
        }

        query = query.Include(x => x.Service);

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
        var orders = await PagedResult<Order>.CreateAsync(query, request.PageIndex, request.PageSize);
        var mapped = orders.Items.Select(x => new Response.Order(
            x.Id,
            x.Customer.FirstName + " " + x.Customer.LastName,
            x.Service.Name,
            x.TotalAmount,
            x.Discount,
            x.FinalAmount,
            x.OrderDate,
            x.Status
        )).ToList();

        var result = new PagedResult<Response.Order>(mapped, orders.PageIndex, orders.PageSize, orders.TotalCount);
        return Result.Success(result);
    }

    private static Expression<Func<Order, object>> GetSortProperty(Query.GetOrdersByCustomerId request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "date" => projection => projection.OrderDate,
            "total amount" => projection => projection.TotalAmount,
            "discount" => projection => projection.Discount,
            "final amount" => projection => projection.FinalAmount,
            _ => projection => projection.CreatedOnUtc
        };
    }
}