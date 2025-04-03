using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleUpdatedDoctorNoteEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleUpdatedDoctorNote>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleUpdatedDoctorNote request,
        CancellationToken cancellationToken)
    {
        var customerSchedule =
            await customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.IdCustomerSchedule);
        if (customerSchedule is null)
            return Result.Failure(new Error("404", "Customer Schedule Not Found !"));

        customerSchedule.DoctorNote = request.DoctorNote;
        customerScheduleRepository.ReplaceOneAsync(customerSchedule);
        return Result.Success();
    }
}