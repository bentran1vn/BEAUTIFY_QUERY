using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetOrderByIdQueryHandler(IRepositoryBase<Order, Guid> orderRepositoryBase)
    : IQueryHandler<Query.GetOrderById, Response.OrderById>
{
    public async Task<Result<Response.OrderById>> Handle(Query.GetOrderById request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch order with necessary relationships
        var order = await orderRepositoryBase.FindSingleAsync(
            x => x.Id == request.Id,
            cancellationToken,
            x => x.LivestreamRoom,
            x => x.CustomerSchedules
        );

        if (order == null)
            return Result.Failure<Response.OrderById>(new Error("404", "Order Not Found"));

        // 2. Map ALL customer schedules (no deduplication)
        var customerSchedules = order.CustomerSchedules
            .Where(cs => cs.Doctor != null) // Filter out null doctors if needed
            .OrderBy(cs => cs.Date)
            .ThenBy(cs => cs.StartTime)
            .Select(cs => new Response.CustomerSchedule(
                cs.Id,
                cs.DoctorId,
                cs.Doctor!.User!.FullName, // Null checks handled by Where filter
                cs.Doctor.User.ProfilePicture,
                cs.Status,
                cs.Date,
                cs.StartTime,
                cs.EndTime))
            .ToList();

        return Result.Success(new Response.OrderById(
            order.Id,
            order.Customer.FullName,
            order.Service.Name,
            order.TotalAmount,
            order.Discount,
            order.DepositAmount,
            order.FinalAmount,
            order.CreatedOnUtc,
            order.Status,
            order.Customer.PhoneNumber,
            order.Customer.Email,
            order.LivestreamRoomId != null,
            order.LivestreamRoomId != null ? order.LivestreamRoom.Name : null,
            customerSchedules // Now includes ALL valid schedules
        ));
    }
}