using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleUpdatedDoctorNoteEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository,
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleUpdatedDoctorNote>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleUpdatedDoctorNote request,
        CancellationToken cancellationToken)
    {
        var customerSchedule =
            await customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.IdCustomerSchedule);
        if (customerSchedule is null)
            return Result.Failure(new Error("404", "Customer Schedule Not Found !"));

        var workingSchedule =
            await workingScheduleRepository.FindOneAsync(x => x.CustomerScheduleId == request.IdCustomerSchedule);
        if (workingSchedule is null)
            return Result.Failure(new Error("404", "Working Schedule Not Found !"));
        workingSchedule.IsNoted = true;
        workingSchedule.Note = request.DoctorNote;
        customerSchedule.DoctorNote = request.DoctorNote;
        await workingScheduleRepository.ReplaceOneAsync(workingSchedule);
        await customerScheduleRepository.ReplaceOneAsync(customerSchedule);
        return Result.Success();
    }
}