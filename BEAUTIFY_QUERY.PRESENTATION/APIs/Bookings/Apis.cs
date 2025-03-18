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
}