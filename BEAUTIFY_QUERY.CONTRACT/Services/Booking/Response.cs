namespace BEAUTIFY_QUERY.CONTRACT.Services.Booking;
public static class Response
{
    public record GetBookingResponse(
        Guid Id,
        string CustomerName,
        TimeSpan Start,
        TimeSpan End,
        string ServiceName,
        string CurrentProcedurePriceType,
        string Status,
        DateOnly date);
}