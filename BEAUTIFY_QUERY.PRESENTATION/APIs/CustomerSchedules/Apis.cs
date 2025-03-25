using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using Microsoft.AspNetCore.Mvc;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.CustomerSchedules;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{verison:apiVersion}/customer-schedules";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("CustomerSchedules").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("customer/{customerName}", StaffCheckInCustomerSchedule)
            .RequireAuthorization()
            .WithName("Staff Check In Customer Schedule")
            .WithSummary("Staff Check In Customer Schedule")
            .WithDescription("Check in customer schedule by staff");
    }

    private static async Task<IResult> StaffCheckInCustomerSchedule(ISender sender,
        [FromRoute] string customerName, [FromQuery] string customerPhone)

    {
        var result = await sender.Send(
            new Query.StaffCheckInCustomerScheduleQuery(customerName, customerPhone));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}