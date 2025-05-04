using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
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
        // Get the current clinic (which should be a parent clinic)
        var currentClinic = await clinicRepository.FindByIdAsync(currentUserService.ClinicId.Value, cancellationToken);
        if (currentClinic == null)
            return Result.Failure<PagedResult<Response.Order>>(new Error("404", "Clinic not found"));

        // Check if the clinic is a parent clinic
        if (currentClinic.IsParent != true)
            return Result.Failure<PagedResult<Response.Order>>(new Error("403",
                "Only parent clinics can access this endpoint"));

        // Get all child clinic IDs
        var childClinicIds = await clinicRepository
            .FindAll(c => c.ParentId == currentUserService.ClinicId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (childClinicIds.Count == 0)
            return Result.Success(new PagedResult<Response.Order>([], 0, request.PageIndex, request.PageSize));

        // Get orders from all child clinics
        var searchTerm = request.SearchTerm?.Trim();
        var query = orderRepository.FindAll(x =>
            x.Service != null &&
            x.Service.ClinicServices != null &&
            x.Service.ClinicServices.Any() &&
            childClinicIds.Contains(x.Service.ClinicServices.First().ClinicId));

        // Execute the initial query to get the base data
        var baseQuery = await query
            .Include(x => x.Customer)
            .Include(x => x.Service)
            .Include(x => x.LivestreamRoom)
            .ToListAsync(cancellationToken);

        // Then filter in memory
        var filteredList = baseQuery.AsEnumerable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            filteredList = filteredList.Where(x =>
                (x.Customer != null && x.Customer.FullName != null &&
                 x.Customer.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (x.Service != null && x.Service.Name != null &&
                 x.Service.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (x.Customer != null && x.Customer.PhoneNumber != null &&
                 x.Customer.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (x.FinalAmount.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                x.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting in memory
        filteredList = request.SortOrder == SortOrder.Descending
            ? filteredList.OrderByDescending(GetSortPropertyInMemory(request))
            : filteredList.OrderBy(GetSortPropertyInMemory(request));

        // Manual paging
        var totalCount = filteredList.Count();
        var pagedItems = filteredList
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to response
        var mapped = pagedItems.Select(x =>
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
            new PagedResult<Response.Order>(mapped, request.PageIndex, request.PageSize, totalCount));
    }

    private static Func<Order, object> GetSortPropertyInMemory(Query.GetClinicOrderBranchesQuery request)
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