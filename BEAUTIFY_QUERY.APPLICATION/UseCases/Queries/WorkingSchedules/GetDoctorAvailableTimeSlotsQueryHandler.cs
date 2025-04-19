using System.Collections.Immutable;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetDoctorAvailableTimeSlotsQueryHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : IQueryHandler<Query.GetDoctorAvailableTimeSlots, IReadOnlyList<Response.GetEmptyScheduleResponse>>
{
    public async Task<Result<IReadOnlyList<Response.GetEmptyScheduleResponse>>> Handle(
        Query.GetDoctorAvailableTimeSlots request, CancellationToken cancellationToken)
    {
        // Get all working schedules for the doctor on the specified date
        var workingSchedules = repository.FilterBy(x =>
                x.DoctorId == request.DoctorId &&
                x.ClinicId == request.ClinicId &&
                x.Date == request.Date &&
                !x.IsDeleted)
            .ToImmutableList();

        // Filter to only include schedules that don't have a customer schedule assigned
        var availableTimeSlots = workingSchedules
            .Where(x => x.CustomerScheduleId == null)
            .Select(x => new Response.GetEmptyScheduleResponse(
                x.DocumentId,
                x.Date,
                x.StartTime,
                x.EndTime
            ))
            .OrderBy(x => x.StartTime)
            .ToList();

        return Result.Success<IReadOnlyList<Response.GetEmptyScheduleResponse>>(availableTimeSlots);
    }
}