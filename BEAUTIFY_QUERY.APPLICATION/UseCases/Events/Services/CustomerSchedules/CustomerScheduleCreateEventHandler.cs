using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.CustomerSchedules;
internal sealed class CustomerScheduleCreateEventHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository)
    : ICommandHandler<DomainEvents.CustomerScheduleCreated>
{
    public async Task<Result> Handle(DomainEvents.CustomerScheduleCreated request, CancellationToken cancellationToken)
    {
        var service = request.entity;

        var customerSchedule = new CustomerScheduleProjection
        {
            DocumentId = service.Id,
            CustomerName = service.CustomerName,
            CustomerId = service.CustomerId,
            StartTime = service.StartTime,
            EndTime = service.EndTime,
            Date = service.Date,
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            DoctorId = service.DoctorId,
            DoctorName = service.DoctorName,
            ClinicId = service.ClinicId,
            ClinicName = service.ClinicName,
            DoctorNote = service.DoctorNote,
            CurrentProcedure = new EntityEvent.ProcedurePriceTypeEntity
            {
                Name = service.CurrentProcedure.Name,
                Id = service.CurrentProcedure.Id,
                StepIndex = service.CurrentProcedure.StepIndex,
                DateCompleted = (DateOnly)service.Date,
                Duration = 0,
            },
            Status = service.Status,
            PendingProcedures = [],
            CompletedProcedures = []
        };
        await customerScheduleRepository.InsertOneAsync(customerSchedule);
        return Result.Success();
    }
}