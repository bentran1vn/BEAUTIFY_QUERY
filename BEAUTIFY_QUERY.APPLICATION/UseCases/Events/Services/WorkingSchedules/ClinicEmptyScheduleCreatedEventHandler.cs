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

                // CRITICAL FIX: Check if records for this shift group already exist
                var existingCount = workingScheduleMongoRepository
                    .FilterBy(filter => filter.ShiftGroupId == shiftGroupId).Count();

                if (existingCount > 0)
                {
                    continue; // Skip this group if records already exist
                }

                // Convert the DateTime to the desired time zone
                var dateTimeInVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var workingSchedule = schedulesForGroup.Select(x => new WorkingScheduleProjection
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
                        CreatedOnUtc = dateTimeInVN,
                    })
                    .ToList();

                // Insert the new schedules into the MongoDB collection
                await workingScheduleMongoRepository.InsertManyAsync(workingSchedule);
            }

            // Handle schedules without ShiftGroupId (if any)
            var schedulesWithoutGroup = request.WorkingScheduleEntities
                .Where(x => !x.ShiftGroupId.HasValue)
                .ToList();


            if (schedulesWithoutGroup.Count <= 0) return Result.Success();
            {
                // Convert the DateTime to the desired time zone
                var dateTimeInVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var workingSchedule = schedulesWithoutGroup.Select(x => new WorkingScheduleProjection
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
                        CreatedOnUtc = dateTimeInVN,
                    })
                    .ToList();

                // Insert the new schedules into the MongoDB collection
                await workingScheduleMongoRepository.InsertManyAsync(workingSchedule);
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