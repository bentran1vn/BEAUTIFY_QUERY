using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
internal sealed class GetOrderByIdQueryHandler(IRepositoryBase<Order, Guid> orderRepositoryBase)
    : IQueryHandler<Query.GetOrderById, Response.OrderById>
{
    public async Task<Result<Response.OrderById>> Handle(Query.GetOrderById request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch order with necessary relationships
        var query = orderRepositoryBase.FindAll(x => x.Id == request.Id
        );

        query = query
            .Include(x => x.OrderFeedback)
            .ThenInclude(x => x.OrderFeedbackMedias)
            .Include(x => x.LivestreamRoom)
            .Include(x => x.CustomerSchedules)
            .ThenInclude(x => x.Doctor)
            .ThenInclude(x => x.User)
            .Include(x => x.CustomerSchedules)
            .ThenInclude(x => x.ProcedurePriceType)
            .ThenInclude(x => x.Procedure)
            .Include(x => x.CustomerSchedules)
            .ThenInclude(x => x.Feedback);

        var order = await query.FirstOrDefaultAsync(cancellationToken);

        if (order == null)
            return Result.Failure<Response.OrderById>(new Error("404", "Order Not Found"));

        // 2. Map ALL customer schedules (no deduplication)
        var customerSchedules = order.CustomerSchedules?
            .Where(cs => cs.Doctor != null) // Filter out null doctors if needed
            .OrderBy(cs => cs.Date)
            .ThenBy(cs => cs.StartTime)
            .Select(cs => new Response.CustomerSchedule(
                cs.Id,
                cs.DoctorId,
                cs.Doctor!.User!.FullName, // Null checks handled by Where filter
                cs.Procedure?.Name ?? cs.ProcedurePriceType?.Procedure?.Name ?? "Unknown Procedure",
                cs.Doctor.User.ProfilePicture,
                cs.Status,
                cs.Date,
                cs.StartTime,
                cs.EndTime,
                cs.Feedback?.Content,
                cs.Feedback?.Rating,
                cs.Feedback?.CreatedOnUtc))
            .ToList();

        bool isFinished = order.CustomerSchedules != null && order.CustomerSchedules.Count > 0 &&
                          order.CustomerSchedules.All(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED);

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
            order.Customer.PhoneNumber ?? "",
            order.Customer.Email,
            order.LivestreamRoomId != null,
            isFinished,
            order.LivestreamRoomId != null ? order.LivestreamRoom.Name : null,
            order.OrderFeedback?.Content,
            order.OrderFeedback?.Rating,
            order.OrderFeedback?.OrderFeedbackMedias?.Select(x => x.MediaUrl).ToList(),
            customerSchedules ?? [] // Now includes ALL valid schedules
        ));
    }
}