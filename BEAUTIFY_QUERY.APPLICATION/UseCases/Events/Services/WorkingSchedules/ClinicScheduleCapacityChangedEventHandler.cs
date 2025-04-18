using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

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
            // Get all schedules associated with the affected ShiftGroupId
            var existingSchedules =
                workingScheduleMongoRepository.FilterBy(filter => filter.ShiftGroupId == notification.ShiftGroupId);

            // Collection to track which schedules need to be updated
            var schedulesToUpdate = new List<WorkingScheduleProjection>();
            // Collection to track which schedules are newly added
            var schedulesToAdd = new List<WorkingScheduleProjection>();
            // Collection to track which schedules need to be removed
            var schedulesToRemove = new List<WorkingScheduleProjection>();

            // Track the schedules we're keeping from the notification
            var idsInNotification = notification.WorkingScheduleEntities.Select(e => e.Id).ToHashSet();

            // Find any schedules that exist in MongoDB but are not in the notification
            // These need to be removed regardless of capacity change direction
            var schedulesToDelete = existingSchedules
                .Where(s => !idsInNotification.Contains(s.DocumentId))
                .ToList();

            schedulesToRemove.AddRange(schedulesToDelete);

            // Process each schedule in the notification
            foreach (var scheduleEntity in notification.WorkingScheduleEntities)
            {
                // Check if this schedule already exists in MongoDB
                var existingSchedule = existingSchedules.FirstOrDefault(s => s.DocumentId == scheduleEntity.Id);

                if (existingSchedule != null)
                {
                    // Update existing schedule
                    existingSchedule.ShiftCapacity = scheduleEntity.ShiftCapacity;
                    schedulesToUpdate.Add(existingSchedule);
                }
                else
                {
                    // Create new schedule projection
                    var newSchedule = new WorkingScheduleProjection
                    {
                        DocumentId = scheduleEntity.Id,
                        ClinicId = scheduleEntity.ClinicId,
                        Date = scheduleEntity.Date,
                        StartTime = scheduleEntity.StartTime,
                        EndTime = scheduleEntity.EndTime,
                        ShiftGroupId = scheduleEntity.ShiftGroupId,
                        ShiftCapacity = scheduleEntity.ShiftCapacity,
                        Status = scheduleEntity.Status,
                        IsDeleted = scheduleEntity.IsDeleted,
                    };
                    schedulesToAdd.Add(newSchedule);
                }
            }

            // Execute updates in batches for better performance
            if (schedulesToUpdate.Count != 0)
            {
                foreach (var workingScheduleProjection in schedulesToUpdate)
                {
                    await workingScheduleMongoRepository.ReplaceOneAsync(
                        workingScheduleProjection);
                }
            }

            // Add new schedules
            if (schedulesToAdd.Count != 0)
            {
                await workingScheduleMongoRepository.InsertManyAsync(schedulesToAdd);
            }

            // Remove schedules
            if (schedulesToRemove.Count != 0)
            {
                await Task.WhenAll(schedulesToRemove.Select(schedule =>
                    workingScheduleMongoRepository.DeleteOneAsync(x => x.DocumentId == schedule.DocumentId)));
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