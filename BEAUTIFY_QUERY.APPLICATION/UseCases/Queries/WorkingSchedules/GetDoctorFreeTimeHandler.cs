using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using Response = BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules.Response;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class
    GetDoctorFreeTimeHandler(IMongoRepository<WorkingScheduleProjection> mongoRepository)
    : IQueryHandler<Query.GetDoctorFreeTime, List<Response.DoctorBusyTimeInADay>>
{
    public async Task<Result<List<Response.DoctorBusyTimeInADay>>> Handle(Query.GetDoctorFreeTime request,
        CancellationToken cancellationToken)
    {
        var schedule = mongoRepository
            .FilterBy(x => x.DoctorId == request.DoctorId && x.Date == request.Date && x.Status == "Working Shift")
            .ToList();

        if (schedule.Count == 0)
            return Result.Failure<List<Response.DoctorBusyTimeInADay>>(new Error("404", "Doctor Not Found !"));

        var doctorBusySchedule = mongoRepository.FilterBy(x =>
                x.DoctorId == request.DoctorId && x.Date == request.Date && x.Status != "Working Shift" &&
                x.Status != "Cancelled")
            .ToList();

        // Start with working shifts as free time
        var freeTimeSlots = schedule.Select(s => new Response.DoctorBusyTimeInADay
        {
            Start = s.StartTime,
            End = s.EndTime,
            Date = request.Date
        }).ToList();

        // For each busy appointment, update the free time slots
        foreach (var busy in doctorBusySchedule)
        {
            var busyStart = busy.StartTime;
            var busyEnd = busy.EndTime;

            // Create a new list for updated free time slots
            var updatedFreeTimeSlots = new List<Response.DoctorBusyTimeInADay>();

            foreach (var free in freeTimeSlots)
            {
                // If busy time doesn't overlap with free time, keep the slot unchanged
                if (busyEnd <= free.Start || busyStart >= free.End)
                {
                    updatedFreeTimeSlots.Add(free);
                    continue;
                }

                // Add the part of free time before the busy time (if any)
                if (busyStart > free.Start)
                {
                    updatedFreeTimeSlots.Add(new Response.DoctorBusyTimeInADay
                    {
                        Start = free.Start,
                        End = busyStart,
                        Date = request.Date
                    });
                }

                // Add the part of free time after the busy time (if any)
                if (busyEnd < free.End)
                {
                    updatedFreeTimeSlots.Add(new Response.DoctorBusyTimeInADay
                    {
                        Start = busyEnd,
                        End = free.End,
                        Date = request.Date
                    });
                }
            }

            // Replace with updated free time slots
            freeTimeSlots = updatedFreeTimeSlots;
        }

        return Result.Success(freeTimeSlots);
    }
}