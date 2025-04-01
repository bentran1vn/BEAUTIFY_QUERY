using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Booking;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetTotalAppointmentQueryHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleMongoRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetTotalAppointment, Response.GetTotalAppointmentResponse>
{
    public async Task<Result<Response.GetTotalAppointmentResponse>> Handle(Query.GetTotalAppointment request,
        CancellationToken cancellationToken)
    {
        // Parse the input month/year (assuming request.date is in "MM/yyyy" format)
        var parts = request.date.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
        {
            return Result.Failure<Response.GetTotalAppointmentResponse>(new Error("400",
                "Invalid date format. Use MM/yyyy."));
        }

        // Create date range for the entire month
        var firstDayOfMonth = new DateOnly(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var filteredAppointments = customerScheduleMongoRepository.FilterBy(x =>
            x.Date >= firstDayOfMonth &&
            x.Date <= lastDayOfMonth &&
            x.ClinicId == currentUserService.ClinicId);

        var dayCounts = filteredAppointments
            .GroupBy(x => x.Date)
            .Select(g => new Response.GetTotalAppointmentResponse.DayCount
            {
                Date = g.Key.Value.ToString("yyyy-MM-dd"),
                Counts = new Response.GetTotalAppointmentResponse.CountDetails
                {
                    Total = g.Count(),
                    Completed = g.Count(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED),
                    Pending = g.Count(x => x.Status == Constant.OrderStatus.ORDER_PENDING),
                    Cancelled = g.Count(x => x.Status == Constant.OrderStatus.ORDER_UNCOMPLETED),
                    InProgress = g.Count(x => x.Status == Constant.OrderStatus.ORDER_IN_PROGRESS)
                }
            })
            .ToList();

        return Result.Success(new Response.GetTotalAppointmentResponse
        {
            Month = request.date,
            Days = dayCounts
        });
    }
}