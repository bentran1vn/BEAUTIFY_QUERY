using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class ClinicEmptyScheduleCreatedEventHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleMongoRepository)
    : ICommandHandler<DomainEvents.ClinicEmptyScheduleCreated>
{
    public async Task<Result> Handle(DomainEvents.ClinicEmptyScheduleCreated request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Group work schedules by ShiftGroupId to handle them properly
            var schedulesByShiftGroup = request.WorkingScheduleEntities
                .Where(x => x.ShiftGroupId.HasValue)
                .GroupBy(x => x.ShiftGroupId.Value);

            foreach (var group in schedulesByShiftGroup)
            {
                var shiftGroupId = group.Key;
                var schedulesForGroup = group.ToList();

                // Get existing schedules for comparison
                var existingSchedules = workingScheduleMongoRepository
                    .FilterBy(filter => filter.ShiftGroupId == shiftGroupId).ToList();

                // If records already exist, we'll do a proper merge instead of skipping entirely
                // This ensures data consistency between SQL and MongoDB
                var schedulesToInsert = new List<WorkingScheduleProjection>();

                foreach (var entity in schedulesForGroup)
                {
                    // Check if this specific schedule already exists
                    var existingSchedule = existingSchedules.FirstOrDefault(e => e.DocumentId == entity.Id);

                    if (existingSchedule != null)
                    {
                        // Skip this individual schedule as it already exists
                        continue;
                    }

                    // Create a new projection with the event timestamp
                    var workingSchedule = new WorkingScheduleProjection
                    {
                        DocumentId = entity.Id,
                        ClinicId = entity.ClinicId,
                        StartTime = entity.StartTime,
                        EndTime = entity.EndTime,
                        Date = entity.Date,
                        Status = entity.Status,
                        ShiftCapacity = entity.ShiftCapacity,
                        ShiftGroupId = entity.ShiftGroupId,
                        IsDeleted = entity.IsDeleted,
                        // Use event timestamp
                        // Use event timestamp
                    };

                    schedulesToInsert.Add(workingSchedule);
                }

                // Insert only new schedules that don't already exist
                if (schedulesToInsert.Count > 0)
                {
                    await workingScheduleMongoRepository.InsertManyAsync(schedulesToInsert);
                }
            }

            // Handle schedules without ShiftGroupId (if any)
            var schedulesWithoutGroup = request.WorkingScheduleEntities
                .Where(x => !x.ShiftGroupId.HasValue)
                .ToList();

            if (schedulesWithoutGroup.Count > 0)
            {
                // Get all existing schedule IDs to avoid duplicates
                var existingIds = workingScheduleMongoRepository
                    .FilterBy(x => x.ShiftGroupId == null)
                    .Select(x => x.DocumentId)
                    .ToHashSet();

                // Filter out schedules that already exist
                var newSchedules = schedulesWithoutGroup
                    .Where(x => !existingIds.Contains(x.Id))
                    .Select(x => new WorkingScheduleProjection
                    {
                        DocumentId = x.Id,
                        ClinicId = x.ClinicId,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        Date = x.Date,
                        Status = x.Status,
                        ShiftCapacity = x.ShiftCapacity,
                        ShiftGroupId = x.ShiftGroupId,
                        IsDeleted = x.IsDeleted,
                        // Use event timestamp
                        // Use event timestamp
                    })
                    .ToList();

                // Insert only new schedules
                if (newSchedules.Count > 0)
                {
                    await workingScheduleMongoRepository.InsertManyAsync(newSchedules);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ClinicEmptyScheduleCreatedError",
                $"Failed to process ClinicEmptyScheduleCreated event: {ex.Message}"));
        }
    }
}