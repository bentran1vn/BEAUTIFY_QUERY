using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleUpdateDateAndTimeEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleUpdateDateAndTime>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleUpdateDateAndTime request,
        CancellationToken cancellationToken)
    {
        var customerSchedule =
            await customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.IdCustomerSchedule);
        if (customerSchedule is null)
            return Result.Failure(new Error("404", "Customer Schedule Not Found !"));

        customerSchedule.StartTime = request.StartTime;
        customerSchedule.EndTime = request.EndTime;
        customerSchedule.Date = request.Date;

        await customerScheduleRepository.ReplaceOneAsync(customerSchedule);
        return Result.Success();
    }
}