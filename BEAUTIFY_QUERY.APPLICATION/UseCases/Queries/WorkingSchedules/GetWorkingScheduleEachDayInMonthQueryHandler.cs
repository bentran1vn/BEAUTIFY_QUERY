using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleEachDayInMonthQueryHandler(
    IMongoRepository<WorkingScheduleProjection> mongoRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWorkingScheduleEachDayInMonth, Response.GetWorkingScheduleEachDayInMonthResponse>
{
    public async Task<Result<Response.GetWorkingScheduleEachDayInMonthResponse>> Handle(
        Query.GetWorkingScheduleEachDayInMonth request, CancellationToken cancellationToken)
    {
        var workingSchedules = await mongoRepository.AsQueryable(x =>
            x.Date.Year == request.Date.Year && x.Date.Month == request.Date.Month &&
            x.DoctorId == currentUserService.UserId).ToListAsync(cancellationToken);

        if (workingSchedules.Count == 0)
            return Result.Failure<Response.GetWorkingScheduleEachDayInMonthResponse>(
                new Error("404", "Working Schedule Not Found !"));

        var response = new Response.GetWorkingScheduleEachDayInMonthResponse
        {
            Year = request.Date.Year,
            Month = request.Date.Month,
            AppointmentCounts = workingSchedules
                .GroupBy(ws => ws.Date)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Result.Success(response);
    }
}