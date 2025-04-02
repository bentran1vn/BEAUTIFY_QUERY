namespace BEAUTIFY_QUERY.CONTRACT.Services.Booking;
public static class Response
{
    public record GetBookingResponse(
        Guid Id,
        string CustomerName,
        TimeSpan? Start,
        TimeSpan? End,
        string ServiceName,
        string CurrentProcedurePriceType,
        string Status,
        DateOnly? date);

    public record GetTotalAppointmentResponse
    {
        public string Month { get; init; } // Format: "yyyy-MM"
        public List<DayCount> Days { get; init; }

        public record DayCount
        {
            public string? Date { get; init; } // Format: "yyyy-MM-dd"
            public CountDetails Counts { get; init; }
        }

        public record CountDetails
        {
            public int Total { get; init; }
            public int Completed { get; init; }
            public int InProgress { get; init; }
            public int Pending { get; init; }
            public int Cancelled { get; init; }
        }
    }

    public class GetBookingWithDateResponse
    {
        public string Date { get; set; }
        public List<AppointmentResponse> Appointments { get; set; }
    }

    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public CustomerResponse Customer { get; set; }
        public ServiceResponse Service { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public DoctorResponse Doctor { get; set; }
        public ClinicResponse Clinic { get; set; }
    }

    public class CustomerResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }

    public class ServiceResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
    }

    public class DoctorResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
    }

    public class ClinicResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
    }
}