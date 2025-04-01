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
}