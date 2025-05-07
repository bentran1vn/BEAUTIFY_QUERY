using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class DoctorScheduleRegisteredEventHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleMongoRepository)
    : ICommandHandler<DomainEvents.DoctorScheduleRegistered>
{
    public async Task<Result> Handle(DomainEvents.DoctorScheduleRegistered request, CancellationToken cancellationToken)
    {
        var listWorkingSchedule = workingScheduleMongoRepository.FilterBy(x => request.WorkingScheduleEntities
            .Any(y => y.Id == x.DocumentId)).ToList();
        if (listWorkingSchedule.Count == 0)
        {
            return Result.Failure(new Error("404", "Not found"));
        }

        foreach (var x in listWorkingSchedule)
        {
            x.DoctorId = request.DoctorId;
            x.DoctorName = request.DoctorName;
            await workingScheduleMongoRepository.ReplaceOneAsync(x);
        }

        return Result.Success();
    }
}