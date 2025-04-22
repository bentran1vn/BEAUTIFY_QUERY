using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
public class ClinicScheduleCapacityChangedEventHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleMongoRepository)
    : ICommandHandler<DomainEvents.ClinicScheduleCapacityChanged>
{
    public async Task<Result> Handle(DomainEvents.ClinicScheduleCapacityChanged notification,
        CancellationToken cancellationToken)
    {
        try
        {
            // The key fix is here: First, delete ALL existing schedules for this shift group
            // This ensures we won't have any duplicates or leftover records
            await workingScheduleMongoRepository.DeleteManyAsync(filter =>
                filter.ShiftGroupId == notification.ShiftGroupId);

            // Now insert all the schedules from the notification as new records
            var schedulesToAdd = notification.WorkingScheduleEntities
                .Select(scheduleEntity => new WorkingScheduleProjection
                {
                    DocumentId = scheduleEntity.Id,
                    ClinicId = scheduleEntity.ClinicId,
                    DoctorId = scheduleEntity.DoctorId,
                    Date = scheduleEntity.Date,
                    StartTime = scheduleEntity.StartTime,
                    EndTime = scheduleEntity.EndTime,
                    ShiftGroupId = scheduleEntity.ShiftGroupId,
                    ShiftCapacity = scheduleEntity.ShiftCapacity,
                    Status = scheduleEntity.Status,
                    IsDeleted = scheduleEntity.IsDeleted,
                    CreatedOnUtc = DateTime.UtcNow, // Set creation timestamp
                    ModifiedOnUtc = DateTime.UtcNow // Set modification timestamp
                })
                .ToList();

            // Insert the new schedules
            if (schedulesToAdd.Count > 0)
            {
                await workingScheduleMongoRepository.InsertManyAsync(schedulesToAdd);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ClinicScheduleCapacityChangedError",
                $"Failed to process ClinicScheduleCapacityChanged event: {ex.Message}"));
        }
    }
}