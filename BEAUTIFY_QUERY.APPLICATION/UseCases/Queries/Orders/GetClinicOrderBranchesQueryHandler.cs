using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
/// <summary>
///    api/v{version:apiVersion}/orders/clinic/branches
/// clinic/branches
/// </summary>
/// <param name="currentUserService"></param>
/// <param name="orderRepository"></param>
/// <param name="clinicRepository"></param>
public sealed class GetClinicOrderBranchesQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<Order, Guid> orderRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository)
    : IQueryHandler<Query.GetClinicOrderBranchesQuery, PagedResult<Response.Order>>
{
    public async Task<Result<PagedResult<Response.Order>>> Handle(Query.GetClinicOrderBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var currentClinic = await ValidateClinicAccessAsync(cancellationToken);
        if (currentClinic.IsFailure)
            return Result.Failure<PagedResult<Response.Order>>(currentClinic.Error);

        // Get child clinic IDs
        var childClinicIds = await GetChildClinicIdsAsync(cancellationToken);

        if (childClinicIds.Count == 0)
            return Result.Success(new PagedResult<Response.Order>([], request.PageIndex, request.PageSize, 0));

        // Build and execute query
        var query = BuildOrdersQuery(childClinicIds, request.SearchTerm);
        query = ApplySorting(query, request);

        // Execute query with paging
        var result = await PagedResult<Order>.CreateAsync(query,
            request.PageIndex,
            request.PageSize);

        // Map to response
        var mapped = result.Items.Select(MapOrderToResponse).ToList();

        return Result.Success(
            new PagedResult<Response.Order>(mapped, result.PageIndex, result.PageSize, result.TotalCount));
    }

    private async Task<Result<Clinic>> ValidateClinicAccessAsync(CancellationToken cancellationToken)
    {
        var currentClinic = await clinicRepository.FindByIdAsync(currentUserService.ClinicId.Value, cancellationToken);
        if (currentClinic == null)
            return Result.Failure<Clinic>(new Error("404", "Clinic not found"));

        return currentClinic.IsParent != true
            ? Result.Failure<Clinic>(new Error("403", "Only parent clinics can access this endpoint"))
            : Result.Success(currentClinic);
    }

    private async Task<List<Guid>> GetChildClinicIdsAsync(CancellationToken cancellationToken)
    {
        return await clinicRepository
            .FindAll(c => c.ParentId == currentUserService.ClinicId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Order> BuildOrdersQuery(List<Guid> childClinicIds, string? searchTerm)
    {
        // Base query for orders from child clinics
        var query = orderRepository.FindAll(x =>
            x.Service != null &&
            x.Service.ClinicServices != null &&
            x.Service.ClinicServices.Count != 0 &&
            childClinicIds.Contains(x.Service.ClinicServices.First().ClinicId));

        // Apply search filter if provided
        searchTerm = searchTerm?.Trim();
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x =>
                (x.Customer.FullName ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (x.Service.Name ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (x.Customer.PhoneNumber ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                x.FinalAmount.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                x.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Include related entities
        return query
            .Include(x => x.Customer)
            .Include(x => x.Service)
            .Include(x => x.LivestreamRoom)
            .Include(x => x.CustomerSchedules);
    }

    private IQueryable<Order> ApplySorting(IQueryable<Order> query, Query.GetClinicOrderBranchesQuery request)
    {
        var sortProperty = GetSortProperty(request);
        return request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(sortProperty)
            : query.OrderBy(sortProperty);
    }

    private static Response.Order MapOrderToResponse(Order x)
    {
        bool isFinished = x.CustomerSchedules != null && x.CustomerSchedules.Count > 0 &&
                          x.CustomerSchedules.All(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED);
        
        return new Response.Order(
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
            x.LivestreamRoomId.HasValue,
            isFinished,
            x.LivestreamRoomId.HasValue ? x.LivestreamRoom.Name : null);
    }

    private static Expression<Func<Order, object>> GetSortProperty(Query.GetClinicOrderBranchesQuery request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "customername" => x => x.Customer.FullName,
            "servicename" => x => x.Service.Name,
            "totalamount" => x => x.TotalAmount,
            "finalamount" => x => x.FinalAmount,
            "orderdate" => x => x.CreatedOnUtc,
            "status" => x => x.Status,
            _ => x => x.CreatedOnUtc
        };
    }
}