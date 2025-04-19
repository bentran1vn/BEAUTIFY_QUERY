using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleDetailQueryHandler(IMongoRepository<WorkingScheduleProjection> mongoRepository)
    : IQueryHandler<Query.GetWorkingScheduleDetail, Response.GetWorkingScheduleDetailResponse>
{
    public async Task<Result<Response.GetWorkingScheduleDetailResponse>> Handle(Query.GetWorkingScheduleDetail request,
        CancellationToken cancellationToken)
    {
        var workingSchedule = await mongoRepository.AsQueryable(x => x.DocumentId == request.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (workingSchedule is null)
            return Result.Failure<Response.GetWorkingScheduleDetailResponse>(
                new Error("404", "Working Schedule Not Found !"));

        var appointment = new Response.GetWorkingScheduleResponseDaily.Appointment
        {
            Id = workingSchedule.DocumentId,
            CustomerName = workingSchedule.CustomerName,
            ServiceName = workingSchedule.ServiceName,
            Date = workingSchedule.Date,
            StartTime = workingSchedule.StartTime,
            EndTime = workingSchedule.EndTime,
            StepIndex = workingSchedule.StepIndex,
            ProcedurePriceTypeName = workingSchedule.CurrentProcedureName,
            Duration = (workingSchedule.EndTime - workingSchedule.StartTime).ToString(),
            Status = workingSchedule.Status,
            CustomerScheduleId = workingSchedule.CustomerScheduleId,
            IsNoted = workingSchedule.IsNoted,
            Note = workingSchedule.Note
        };

        var response = new Response.GetWorkingScheduleDetailResponse
        {
            Appointment = appointment
        };


        return Result.Success(response);
    }
}