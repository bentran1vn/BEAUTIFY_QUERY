using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class
    CustomerScheduleDoctorChangedEventHandler(IMongoRepository<CustomerScheduleProjection> mongoRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleDoctorChanged>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleDoctorChanged request,
        CancellationToken cancellationToken)
    {
        foreach (var Id in request.IdCustomerSchedules)
        {
            var customerSchedule = await mongoRepository.FindOneAsync(x => x.DocumentId == Id);
            if (customerSchedule == null) continue;
            customerSchedule.DoctorId = request.IdDoctor;
            customerSchedule.DoctorName = request.DoctorName;
            await mongoRepository.ReplaceOneAsync(customerSchedule);
        }

        return Result.Success();
    }
}