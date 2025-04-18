using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class ClinicEmptyScheduleCreatedEventHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleMongoRepository)
    : ICommandHandler<DomainEvents.ClinicEmptyScheduleCreated>
{
    public async Task<Result> Handle(DomainEvents.ClinicEmptyScheduleCreated request,
        CancellationToken cancellationToken)
    {
        // Convert the DateTime to the desired time zone
        var dateTimeInVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        var workingSchedule = request.WorkingScheduleEntities.Select(x => new WorkingScheduleProjection
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
        return Result.Success();
    }
}