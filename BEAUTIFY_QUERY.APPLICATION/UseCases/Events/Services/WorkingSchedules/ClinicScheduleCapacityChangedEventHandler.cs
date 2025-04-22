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

            // Step 2: Delete ALL existing records for this shift group
            var deleteResult =
                workingScheduleMongoRepository.FilterBy(x => x.ShiftGroupId == notification.ShiftGroupId).Count();
            await workingScheduleMongoRepository.DeleteManyAsync(filter =>
                filter.ShiftGroupId == notification.ShiftGroupId);

            // Step 3: Transform the entities to projections
            var dateTimeInVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

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
                    CreatedOnUtc = dateTimeInVN, // Use the same time zone as in ClinicEmptyScheduleCreatedEventHandler
                    ModifiedOnUtc = dateTimeInVN
                })
                .ToList();

            // Step 4: Double check we don't have duplicates before inserting
            var uniqueSchedules = schedulesToAdd
                .GroupBy(s => s.DocumentId)
                .Select(g => g.First())
                .ToList();

            if (uniqueSchedules.Count != schedulesToAdd.Count)
            {
                schedulesToAdd = uniqueSchedules;
            }

            // Step 5: Insert the new schedules
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