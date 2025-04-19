using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Booking;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetTotalAppointmentQueryHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleMongoRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetTotalAppointment, Response.GetTotalAppointmentResponse>
{
    public async Task<Result<Response.GetTotalAppointmentResponse>> Handle(Query.GetTotalAppointment request,
        CancellationToken cancellationToken)
    {
        if (!TryParseDate(request.date, out var firstDayOfMonth, out var lastDayOfMonth))
            return Result.Failure<Response.GetTotalAppointmentResponse>(new Error("400",
                "Invalid date format. Use MM-yyyy."));

        var filteredAppointments = await GetFilteredAppointmentsAsync(firstDayOfMonth, lastDayOfMonth);
        var dayCounts = CalculateDayCounts(filteredAppointments);
        return Result.Success(new Response.GetTotalAppointmentResponse
        {
            Month = request.date,
            Days = dayCounts
        });
    }

    private static bool TryParseDate(string dateString, out DateOnly firstDayOfMonth, out DateOnly lastDayOfMonth)
    {
        firstDayOfMonth = default;
        lastDayOfMonth = default;

        var parts = dateString.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
            return false;

        firstDayOfMonth = new DateOnly(year, month, 1);
        lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        return true;
    }

    private async Task<IEnumerable<CustomerScheduleProjection>> GetFilteredAppointmentsAsync(DateOnly firstDay,
        DateOnly lastDay)
    {
        return customerScheduleMongoRepository.FilterBy(x =>
            x.Date >= firstDay &&
            x.Date <= lastDay &&
            x.ClinicId == currentUserService.ClinicId);
    }

    private static List<Response.GetTotalAppointmentResponse.DayCount> CalculateDayCounts(
        IEnumerable<CustomerScheduleProjection> appointments)
    {
        return appointments
            .GroupBy(x => x.Date)
            .Where(g => g.Any())
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
    }
}