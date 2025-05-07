using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;

public class GetOrderSystemsQueryHandler(
    IRepositoryBase<Order, Guid> orderRepositoryBase,
    IRepositoryBase<ClinicTransaction, Guid> clinicTransactionRepositoryBase)
    : IQueryHandler<Query.GetOrderSystems, PagedResult<Response.OrderSystem>>
{
    public async Task<Result<PagedResult<Response.OrderSystem>>> Handle(Query.GetOrderSystems request, CancellationToken cancellationToken)
    {
        var query = BuildQuery(request);
        var orders = await PagedResult<OrderWithTransaction>.CreateAsync(query, request.PageIndex, request.PageSize);
        
        var result = MapToResponse(orders);
        
        return Result.Success(result);
    }
    
    private IQueryable<OrderWithTransaction> BuildQuery(Query.GetOrderSystems request)
    {
        var tranQuery = clinicTransactionRepositoryBase.FindAll(x => x.IsDeleted == false);

        tranQuery = tranQuery
            .Include(x => x.Clinic)
            .ThenInclude(x => x.Parent);
        
        var query = orderRepositoryBase.FindAll(x => x.IsDeleted == false)
            .Include(x => x.Service)
            .Include(x => x.LivestreamRoom)
            .Include(x => x.Customer); // Include Customer for mapping response
        
        var joinedQuery = query.Join(
            tranQuery,
            order => order.Id,
            transaction => transaction.OrderId,
            (order, transaction) => new OrderWithTransaction { 
                Order = order, 
                Transaction = transaction,
                Clinic = transaction.Clinic
            }
        );

        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) 
            ApplySearchFilterToJoined(ref joinedQuery, request.SearchTerm.Trim());
    
        // Apply sorting
        joinedQuery = ApplySortingToJoined(joinedQuery, request);
    
        return joinedQuery;
    }
    
    private class OrderWithTransaction
    {
        public Order Order { get; set; }
        public ClinicTransaction Transaction { get; set; }
        public Clinic? Clinic { get; set; }
    }
    
    private static void ApplySearchFilterToJoined(ref IQueryable<OrderWithTransaction> query, string searchTerm)
    {
        var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return;

        var part1 = parts[0].Trim();
        var part2 = parts[1].Trim();

        if (DateOnly.TryParse(part1, out var dateFrom) && DateOnly.TryParse(part2, out var dateTo))
            query = query.Where(x => x.Order.OrderDate >= dateFrom && x.Order.OrderDate <= dateTo);
        else if (decimal.TryParse(part1, out var priceFrom) && decimal.TryParse(part2, out var priceTo))
            query = query.Where(x => (x.Order.FinalAmount >= priceFrom && x.Order.FinalAmount <= priceTo) ||
                                     (x.Order.Discount >= priceFrom && x.Order.Discount <= priceTo) ||
                                     (x.Order.TotalAmount >= priceFrom && x.Order.TotalAmount <= priceTo));
    }
    
    // Apply sorting to the joined query
    private static IQueryable<OrderWithTransaction> ApplySortingToJoined(IQueryable<OrderWithTransaction> query, Query.GetOrderSystems request)
    {
        return request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortPropertyForJoined(request))
            : query.OrderBy(GetSortPropertyForJoined(request));
    }
    
    private static PagedResult<Response.OrderSystem> MapToResponse(PagedResult<OrderWithTransaction> joinedResults)
    {
        var mapped = joinedResults.Items.Select(x => new Response.OrderSystem(
                x.Order.Id,
                x.Clinic.Parent.Id,
                x.Clinic.Parent.Name,
                x.Clinic.Parent.Address,
                x.Clinic.Parent.PhoneNumber,
                x.Clinic.Id,
                x.Clinic.Name,
                x.Clinic.Address,
                x.Clinic.PhoneNumber,
                x.Order.Customer.FullName,
                x.Order.Service.Name,
                x.Order.TotalAmount,
                x.Order.Discount,
                x.Order.DepositAmount,
                x.Order.FinalAmount,
                x.Order.CreatedOnUtc,
                x.Order.Status,
                x.Order.Customer.PhoneNumber,
                x.Order.Customer.Email,
                x.Order.LivestreamRoomId != null,
                x.Order.LivestreamRoomId != null ? x.Order.LivestreamRoom.Name : null
            ))
            .OrderBy(x => x.Status)
            .ToList();

        return new PagedResult<Response.OrderSystem>(
            mapped, joinedResults.PageIndex, joinedResults.PageSize, joinedResults.TotalCount);
    }
    
    // Get the property to sort by
    private static Expression<Func<OrderWithTransaction, object>> GetSortPropertyForJoined(Query.GetOrderSystems request)
    {
        return request.SortColumn switch
        {
            "totalAmount" => projection => projection.Order.TotalAmount,
            "discount" => projection => projection.Order.Discount,
            "finalAmount" => projection => projection.Order.FinalAmount,
            "orderDate" => projection => projection.Order.OrderDate,
            "clinicName" => projection => projection.Clinic.Name,
            "brandName" => projection => projection.Clinic.Parent.Name,
            _ => projection => projection.Order.CreatedOnUtc
        };
    }
}