using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleUpdateDateAndTimeEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleUpdateDateAndTimeAndStatus>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleUpdateDateAndTimeAndStatus request,
        CancellationToken cancellationToken)
    {
        var customerSchedule =
            await customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.IdCustomerSchedule);
        if (customerSchedule is null)
            return Result.Failure(new Error("404", "Customer Schedule Not Found !"));

        customerSchedule.StartTime = request.StartTime;
        customerSchedule.EndTime = request.EndTime;
        customerSchedule.Date = request.Date;
        customerSchedule.Status = request.Status;

        await customerScheduleRepository.ReplaceOneAsync(customerSchedule);
        return Result.Success();
    }
}