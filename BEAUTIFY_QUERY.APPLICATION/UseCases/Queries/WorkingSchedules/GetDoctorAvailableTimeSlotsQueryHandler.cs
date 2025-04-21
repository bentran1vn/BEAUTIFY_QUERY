/*using System.Collections.Immutable;
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
}*/
///<summary>
/// /api/v1/working-schedules/doctors/available-times
/// </summary>

using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetDoctorAvailableTimeSlotsQueryHandler(
    IMongoRepository<WorkingScheduleProjection> repository,
    IMongoRepository<ClinicServiceProjection> clinicServiceRepository,
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository)
    : IQueryHandler<Query.GetDoctorAvailableTimeSlots, IReadOnlyList<Response.GetEmptyScheduleResponse>>
{
    public async Task<Result<IReadOnlyList<Response.GetEmptyScheduleResponse>>> Handle(
        Query.GetDoctorAvailableTimeSlots request, CancellationToken cancellationToken)
    {
        // Get duration and clinic ID logic remains the same
        int duration;
        Guid clinicId;
        if (request.IsCustomerSchedule)
        {
            // Existing customer schedule logic...
            var customerSchedule = await
                customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.serviceIdOrCustomerScheduleId);
            if (customerSchedule == null)
                return Result.Failure<IReadOnlyList<Response.GetEmptyScheduleResponse>>(new Error("404",
                    ErrorMessages.CustomerSchedule.CustomerScheduleNotFound));
            /*var nextCustomerSchedule = await
                customerScheduleRepository.FindOneAsync(x =>
                    x.DoctorId == customerSchedule.DoctorId &&
                    x.ServiceId == customerSchedule.ServiceId &&
                    x.OrderId == customerSchedule.OrderId &&
                    x.CurrentProcedure.StepIndex == (int.Parse(customerSchedule.CurrentProcedure.StepIndex) + 1
                    ).ToString());
            if (nextCustomerSchedule == null)
            {
                return Result.Failure<IReadOnlyList<Response.GetEmptyScheduleResponse>>(new Error("404",
                    ErrorMessages.CustomerSchedule.NextCustomerScheduleNotFound));
            }

            duration = nextCustomerSchedule.CurrentProcedure.Duration;*/
            duration = customerSchedule.CurrentProcedure.Duration;
            clinicId = customerSchedule.ClinicId.Value;
        }
        else
        {
            var service = await
                clinicServiceRepository.FindOneAsync(x => x.DocumentId == request.serviceIdOrCustomerScheduleId);
            if (service == null)
                return Result.Failure<IReadOnlyList<Response.GetEmptyScheduleResponse>>(new Error("404",
                    ErrorMessages.Service.ServiceNotActive));

            duration = service.Procedures
                .Where(x => x.StepIndex == 1)
                .SelectMany(p => p.ProcedurePriceTypes)
                .OrderByDescending(ppt => ppt.Duration)
                .FirstOrDefault().Duration;
            clinicId = request.ClinicId.Value;
        }

        var allSchedules = repository.FilterBy(x =>
            x.DoctorId == request.DoctorId &&
            x.ClinicId == clinicId &&
            x.Date == request.Date &&
            !x.IsDeleted);

        // Group schedules by shift groups
        var shiftGroups = allSchedules
            .GroupBy(x => x.ShiftGroupId)
            .Select(g => new ShiftGroupData(
                g.Key,
                g.Min(x => x.StartTime),
                g.Max(x => x.EndTime),
                g.Where(x => x.CustomerScheduleId != null).OrderBy(x => x.StartTime).ToList()
            ))
            .ToList();

        var availableSlots = new List<Response.GetEmptyScheduleResponse>();

        foreach (var group in shiftGroups)
        {
            var currentStart = group.GroupStart;
            var bookedSlots = group.BookedAppointments;

            // If there are no booked appointments in this shift, check if the entire shift is long enough
            if (!bookedSlots.Any())
            {
                var totalDuration = (group.GroupEnd - group.GroupStart).TotalMinutes;
                if (totalDuration >= duration)
                {
                    availableSlots.Add(CreateAvailableSlot(
                        request.Date,
                        group.GroupStart,
                        group.GroupEnd
                    ));
                }

                continue;
            }

            foreach (var booked in bookedSlots)
            {
                // Check if there's enough time between current position and next booking
                var availableDuration = (booked.StartTime - currentStart).TotalMinutes;
                if (availableDuration >= duration)
                {
                    availableSlots.Add(CreateAvailableSlot(
                        request.Date,
                        currentStart,
                        booked.StartTime
                    ));
                }

                currentStart = booked.EndTime > currentStart ? booked.EndTime : currentStart;
            }

            // Check if there's enough time after the last booking
            var remainingDuration = (group.GroupEnd - currentStart).TotalMinutes;
            if (remainingDuration >= duration)
            {
                availableSlots.Add(CreateAvailableSlot(
                    request.Date,
                    currentStart,
                    group.GroupEnd
                ));
            }
        }

        // Add completely free shifts not included in any group - only if they're long enough
        var completelyFree = allSchedules
            .Where(x => x.CustomerScheduleId == null && !x.ShiftGroupId.HasValue)
            .Where(x => (x.EndTime - x.StartTime).TotalMinutes >= duration)
            .Select(x => new Response.GetEmptyScheduleResponse(
                x.Date,
                x.StartTime,
                x.EndTime
            ));

        availableSlots.AddRange(completelyFree);

        return Result.Success<IReadOnlyList<Response.GetEmptyScheduleResponse>>(
            availableSlots.OrderBy(x => x.StartTime).ToList());
    }

    private static Response.GetEmptyScheduleResponse CreateAvailableSlot(
        DateOnly date,
        TimeSpan start,
        TimeSpan end)
    {
        // In a real implementation, you'd need to generate proper IDs or handle them differently
        return new Response.GetEmptyScheduleResponse(
            date,
            start,
            end
        );
    }

    private record ShiftGroupData(
        Guid? ShiftGroupId,
        TimeSpan GroupStart,
        TimeSpan GroupEnd,
        List<WorkingScheduleProjection> BookedAppointments
    );
}