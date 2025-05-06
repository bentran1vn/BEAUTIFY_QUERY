using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class DoctorScheduleStatusChangedEventHandler(
    IMongoRepository<WorkingScheduleProjection> mongoRepository)
    : ICommandHandler<DomainEvents.DoctorScheduleStatusChanged>
{
    public async Task<Result> Handle(DomainEvents.DoctorScheduleStatusChanged request,
        CancellationToken cancellationToken)
    {
        foreach (var x in request.WorkingScheduleId)
        {
            var workingSchedule = await mongoRepository.FindOneAsync(x => x.DocumentId.Equals(x));
            if (workingSchedule != null)
            {
                workingSchedule.Status = request.Status;
                await mongoRepository.ReplaceOneAsync(workingSchedule);
            }
            else
            {
                return Result.Failure(new Error("400", "Working schedule not found"));
            }
        }

        return Result.Success();
    }
}