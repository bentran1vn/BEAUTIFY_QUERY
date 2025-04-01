using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Booking;
public static class Query
{
    public record GetBookingPagedResult(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetBookingResponse>>;

    public record GetTotalAppointment(
        string date) : IQuery<Response.GetTotalAppointmentResponse>;
    
   // public record GetBookingWithDate(string Date): IQuery<Response.GetBookingWithDateResponse>;
}