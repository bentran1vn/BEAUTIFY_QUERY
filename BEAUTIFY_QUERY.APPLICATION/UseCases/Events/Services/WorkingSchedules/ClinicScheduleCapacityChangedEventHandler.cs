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
            // Step 1: Collect all IDs that will be included in the updated set
            var updatedScheduleIds = notification.WorkingScheduleEntities
                .Select(e => e.Id)
                .ToHashSet();

            // Step 2: First find all existing records for this shift group
            var existingRecords = workingScheduleMongoRepository.FilterBy(x =>
                x.ShiftGroupId == notification.ShiftGroupId).ToList();

            // Step 3: Delete existing records only after we've retrieved them
            await workingScheduleMongoRepository.DeleteManyAsync(filter =>
                filter.ShiftGroupId == notification.ShiftGroupId);

            // Step 4: Transform the entities to projections
            // Use the timestamp from the event if available, otherwise maintain creation time from existing records
            var schedulesToAdd = (from scheduleEntity in notification.WorkingScheduleEntities
                let existingRecord = existingRecords.FirstOrDefault(r => r.DocumentId == scheduleEntity.Id)
                select new WorkingScheduleProjection
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
                    // Preserve creation time if record existed, otherwise use current time

                    // Always use event timestamp for modifications
                }).ToList();

            // Step 5: Double check for duplicates
            var uniqueSchedules = schedulesToAdd
                .GroupBy(s => s.DocumentId)
                .Select(g => g.First())
                .ToList();

            // Step 6: Insert the new schedules
            if (uniqueSchedules.Count > 0)
            {
                await workingScheduleMongoRepository.InsertManyAsync(uniqueSchedules);
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