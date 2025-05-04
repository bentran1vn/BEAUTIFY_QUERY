namespace BEAUTIFY_QUERY.CONTRACT.Services.Booking;
public static class Response
{
    public record GetBookingResponse(
        Guid Id,
        string CustomerName,
        TimeSpan? Start,
        TimeSpan? End,
        string ServiceName,
        Guid ProcedureId,
        string StepIndex,
        string Name,
        int Duration,
        DateOnly? DateCompleted,
        string Status,
        DateOnly? date);

    public record GetBookingDetailByIdResponse
    {
        public Guid Id { get; init; }
        public string CustomerName { get; init; }
        public DateOnly? Date { get; init; }
        public TimeSpan? StartTime { get; init; }
        public TimeSpan? EndTime { get; init; }
        public string Duration { get; init; }
        public string DoctorNote { get; init; }
        public string Status { get; init; }
        public ServiceResponse Service { get; init; }
        public DoctorResponse Doctor { get; init; }
        public ClinicResponse Clinic { get; init; }
        public List<ProcedureHistory> ProcedureHistory { get; init; }
    }

    public record ProcedureHistory
    {
        public required string ProcedureName { get; init; }
        public string StepIndex { get; init; }
        public string ProcedurePriceType { get; init; }
        public int Duration { get; init; }
        public DateOnly? DateCompleted { get; init; }
        public TimeSpan? TimeCompleted { get; init; }
        public string Status { get; init; }
    }

    public class ProcedurePriceTypeEntity
    {
        public DateOnly? DateCompleted;
        public int Duration;
        public Guid Id;
        public string Name;
        public string StepIndex;
    }

    public record GetTotalAppointmentResponse
    {
        public string Month { get; init; }
        public List<DayCount> Days { get; init; }

        public record DayCount
        {
            public string? Date { get; init; }
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

        public ICollection<string> ImageUrls { get; set; }
    }

    public class DoctorResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<CertificateResponse>? Certificates { get; set; }
    }

    public class CertificateResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ClinicResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
}