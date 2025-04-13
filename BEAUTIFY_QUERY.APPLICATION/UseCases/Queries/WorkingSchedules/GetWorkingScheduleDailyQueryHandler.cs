using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleDailyQueryHandler(
    IMongoRepository<WorkingScheduleProjection> mongoRepository,
    ICurrentUserService currentUserService) : IQueryHandler<Query.GetWorkingScheduleDaily,
    IReadOnlyList<Response.GetWorkingScheduleResponseDaily>>
{
    public async Task<Result<IReadOnlyList<Response.GetWorkingScheduleResponseDaily>>> Handle(
        Query.GetWorkingScheduleDaily request, CancellationToken cancellationToken)
    {
        var workingSchedules = await mongoRepository.AsQueryable(x =>
            x.Date == request.Date && x.DoctorId == currentUserService.UserId).ToListAsync(cancellationToken);

        if (workingSchedules.Count == 0)
            return Result.Failure<IReadOnlyList<Response.GetWorkingScheduleResponseDaily>>(
                new Error("404", "Working Schedule Not Found !"));

        var response = workingSchedules
            .GroupBy(ws => ws.Date)
            .Select(group => new Response.GetWorkingScheduleResponseDaily
            {
                Date = group.Key,
                Appointments = group.Select(ws => new Response.GetWorkingScheduleResponseDaily.Appointment
                {
                    Id = ws.DocumentId,
                    CustomerName = ws.CustomerName,
                    ServiceName = ws.ServiceName,
                    Date = ws.Date,
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                    StepIndex = ws.StepIndex,
                    ProcedurePriceTypeName = ws.CurrentProcedureName,
                    Duration = (ws.EndTime - ws.StartTime).ToString(),
                    Status = ws.Status,
                    CustomerScheduleId = ws.CustomerScheduleId,
                    IsNoted = ws.IsNoted
                }).ToList()
            })
            .ToList();

        return Result.Success<IReadOnlyList<Response.GetWorkingScheduleResponseDaily>>(response);
    }
}