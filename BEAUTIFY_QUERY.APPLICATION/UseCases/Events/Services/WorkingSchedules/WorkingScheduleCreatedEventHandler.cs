using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class WorkingScheduleCreatedEventHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : ICommandHandler<DomainEvents.WorkingScheduleCreated>
{
    public async Task<Result> Handle(DomainEvents.WorkingScheduleCreated request, CancellationToken cancellationToken)
    {
        var workingSchedule = request.WorkingScheduleEntities.Select(x => new WorkingScheduleProjection
            {
                DoctorName = request.DoctorName,
                DocumentId = x.Id,
                ClinicId = x.ClinicId,
                DoctorId = x.DoctorClinicId,
                StartTime = x.StartTime,
                CustomerScheduleId = x.CustomerScheduleId.Value,
                EndTime = x.EndTime,
                Date = x.Date,
                //IsNoted = x.Note,
                Status = x.Status,
                StepIndex = x.CustomerScheduleEntity.CurrentProcedure.StepIndex,
                CustomerName = x.CustomerScheduleEntity.CustomerName,
                CustomerId = x.CustomerScheduleEntity.CustomerId,
                ServiceId = x.CustomerScheduleEntity.ServiceId,
                ServiceName = x.CustomerScheduleEntity.ServiceName,
                CurrentProcedureName = x.CustomerScheduleEntity.CurrentProcedure.Name,
                IsDeleted = x.IsDeleted
            })
            .ToList();
        await repository.InsertManyAsync(workingSchedule);
        return Result.Success();
    }
}