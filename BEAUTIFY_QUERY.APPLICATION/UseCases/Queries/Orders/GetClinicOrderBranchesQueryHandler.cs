using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetClinicOrderBranchesQueryHandler(
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
            return Result.Failure<PagedResult<Response.Order>>(new Error("403", "Only parent clinics can access this endpoint"));

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
            childClinicIds.Contains(x.Service.ClinicServices.FirstOrDefault().ClinicId));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x => x.Customer.FullName.Contains(searchTerm) ||
                                     x.Service.Name.Contains(searchTerm) ||
                                     x.Customer.PhoneNumber.Contains(searchTerm) ||
                                     x.FinalAmount.ToString().Contains(searchTerm));
        }

        // Apply sorting
        query = request.SortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        // Get paged result
        var orders = await PagedResult<Order>.CreateAsync(query, request.PageIndex, request.PageSize);

        // Map to response
        var mapped = orders.Items.Select(x =>
                new Response.Order(
                    x.Id,
                    x.Customer.FullName,
                    x.Service.Name,
                    x.TotalAmount,
                    x.Discount,
                    x.FinalAmount,
                    x.OrderDate,
                    x.Status,
                    x.Customer.PhoneNumber,
                    x.Customer.Email,
                    x.LivestreamRoomId != null,
                    x.LivestreamRoomId != null ? x.LivestreamRoom.Name : null))
            .ToList();

        return Result.Success(
            new PagedResult<Response.Order>(mapped, orders.TotalCount, orders.PageIndex, orders.PageSize));
    }

    private static Expression<Func<Order, object>> GetSortProperty(Query.GetClinicOrderBranchesQuery request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "customername" => x => x.Customer.FullName,
            "servicename" => x => x.Service.Name,
            "totalamount" => x => x.TotalAmount,
            "finalamount" => x => x.FinalAmount,
            "orderdate" => x => x.OrderDate,
            "status" => x => x.Status,
            _ => x => x.OrderDate
        };
    }
}
