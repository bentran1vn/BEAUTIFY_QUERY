namespace BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
public static class Response
{
    public class ShiftGroup
    {
        public Guid Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly Date { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public Guid ClinicId { get; set; }
        public List<GetWorkingScheduleResponse> WorkingSchedules { get; set; } = [];
    }

    public class GetWorkingScheduleResponse_Son
    {
        public Guid WorkingScheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateOnly Date { get; set; }
        public string? Status { get; set; }
        public string StepIndex { get; set; }
        public string CustomerName { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public Guid? CustomerScheduleId { get; set; }
        public string CurrentProcedureName { get; set; }
    }

    public class GetWorkingScheduleResponse
    {
        public Guid WorkingScheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Status { get; set; }
        public string StepIndex { get; set; }
        public string CustomerName { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public Guid? CustomerScheduleId { get; set; }
        public string CurrentProcedureName { get; set; }
    }

    public class DoctorBusyTimeInADay
    {
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
        public DateOnly? Date { get; set; }
    }

    public class GetWorkingScheduleEachDayInMonthResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public Dictionary<DateOnly, int> AppointmentCounts { get; set; } = new();
    }

    public class GetWorkingScheduleResponseDaily
    {
        public DateOnly Date { get; set; }
        public List<Appointment> Appointments { get; set; } = [];

        public class Appointment
        {
            public Guid Id { get; set; }
            public string CustomerName { get; set; }
            public string ServiceName { get; set; }
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string StepIndex { get; set; }
            public string ProcedurePriceTypeName { get; set; }
            public string Duration { get; set; }
            public string Status { get; set; }
            public Guid? CustomerScheduleId { get; set; }
            public bool? IsNoted { get; set; }
            public string? Note { get; set; }
        }
    }

    public record GetEmptyScheduleResponse(
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime
    );

    public record GetEmptyScheduleResponseWithId(
        Guid WorkingScheduleId,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime
    );


    public record GetScheduleResponseForStaff(
        Guid ShiftGroupId,
        int Capacity,
        int NumberOfDoctors,
        int NumberOfCustomers,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime
    );

    public class GetWorkingScheduleDetailResponse
    {
        public GetWorkingScheduleResponseDaily.Appointment Appointment { get; set; } = new();
    }
}