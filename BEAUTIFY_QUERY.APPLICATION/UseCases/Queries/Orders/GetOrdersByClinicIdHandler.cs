using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

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
            query = query.Where(x => x.Customer.FullName.Contains(searchTerm) ||
                                     x.Service.Name.Contains(searchTerm) ||
                                     x.Customer.PhoneNumber.Contains(searchTerm) ||
                                     x.FinalAmount.ToString().Contains(searchTerm));
        }

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
                    x.OrderDate,
                    x.Status,
                    x.Customer.PhoneNumber,
                    x.Customer.Email,
                    x.LivestreamRoomId != null,
                    x.LivestreamRoomId != null ? x.LivestreamRoom.Name : null))
            .ToList();

        return Result.Success(
            new PagedResult<Response.Order>(mapped, orders.PageIndex, orders.PageSize, orders.TotalCount));
    }
}