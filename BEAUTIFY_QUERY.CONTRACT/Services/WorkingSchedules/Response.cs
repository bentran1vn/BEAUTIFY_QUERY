namespace BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
public static class Response
{
    public class GetWorkingScheduleResponse
    {
        public Guid? WorkingScheduleId { get; set; }
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
        public DateOnly? Date { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
    }

    public class DoctorBusyTimeInADay
    {
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
        public DateOnly? Date { get; set; }
    }
}