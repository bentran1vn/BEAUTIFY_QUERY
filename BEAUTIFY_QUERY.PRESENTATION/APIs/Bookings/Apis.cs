using BEAUTIFY_QUERY.CONTRACT.Services.Booking;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Bookings;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/bookings";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Bookings")
            .MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetBookingPagedResult).RequireAuthorization();
        gr1.MapGet("appointments/total", GetTotalAppointment).RequireAuthorization().WithSummary("mm-yyyy");
        gr1.MapGet("appointments/{date}", GetBookingWithDate).RequireAuthorization();
        gr1.MapGet("{id:guid}", GetBookingDetailById).RequireAuthorization();
    }


    private static async Task<IResult> GetBookingDetailById(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetBookingDetailById(id));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }

    private static async Task<IResult> GetBookingPagedResult(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetBookingPagedResult(searchTerm, sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder), pageNumber, pageSize));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }

    private static async Task<IResult> GetTotalAppointment(ISender sender, string Date)
    {
        var result = await sender.Send(new Query.GetTotalAppointment(Date));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }

    private static async Task<IResult> GetBookingWithDate(ISender sender, string date)
    {
        var result = await sender.Send(new Query.GetBookingWithDate(date));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
}