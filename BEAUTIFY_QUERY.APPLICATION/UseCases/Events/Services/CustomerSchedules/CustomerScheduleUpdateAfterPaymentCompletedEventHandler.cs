using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleUpdateAfterPaymentCompletedEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleUpdateAfterPaymentCompleted>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleUpdateAfterPaymentCompleted request,
        CancellationToken cancellationToken)
    {
        var customerSchedule = await customerRepository.FindOneAsync(x => x.DocumentId == request.IdCustomerSchedule);

        if (customerSchedule is null)
            return Result.Failure(new Error("404", "Customer Schedule Not Found !"));
        customerSchedule.Status = request.Status;

        await customerRepository.ReplaceOneAsync(customerSchedule);
        return Result.Success();
    }
}