using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class CheckIfNextCustomerScheduleIsNotQueryHandler(
    IMongoRepository<CustomerScheduleProjection> mongoRepository,
    IRepositoryBase<Service, Guid> serviceRepository)
    : IQueryHandler<Query.CheckIfNextCustomerScheduleIsNotScheduledYet, string>
{
    public async Task<Result<string>> Handle(Query.CheckIfNextCustomerScheduleIsNotScheduledYet request,
        CancellationToken cancellationToken)
    {
        var customerSchedule = await mongoRepository.FindOneAsync(x => x.DocumentId == request.CustomerScheduleId);
        if (customerSchedule is null)
            return Result.Failure<string>(new Error("404", "Customer Schedule Not Found !"));

        var service = await serviceRepository.FindByIdAsync(customerSchedule.ServiceId.Value, cancellationToken);
        if (service is null)
            return Result.Failure<string>(new Error("404", "Service Not Found !"));
        if (service.Procedures.Count.ToString() == customerSchedule.CurrentProcedure.StepIndex)
            return Result.Success("Last Step");

        var nextStep = int.Parse(customerSchedule.CurrentProcedure.StepIndex) + 1;
        var nextCustomerSchedule = await mongoRepository.AsQueryable(x =>
                x.OrderId == customerSchedule.OrderId &&
                x.CurrentProcedure.StepIndex == nextStep.ToString())
            .FirstOrDefaultAsync(cancellationToken);
        if (nextCustomerSchedule is null)
            return Result.Failure<string>(new Error("404", "Next Customer Schedule Not Found !"));
        return Result.Success(nextCustomerSchedule.Date is null
            ? "Need to schedule for next step"
            : "Already scheduled for next step");
    }
}