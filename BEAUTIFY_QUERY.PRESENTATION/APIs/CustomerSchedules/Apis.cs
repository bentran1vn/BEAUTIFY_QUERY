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
        gr1.MapGet("clinic", GetAllCustomerSchedule)
            .RequireAuthorization(Constant.Role.CLINIC_STAFF)
            .WithName("Get All Customer Schedule")
            .WithSummary("Get All Customer Schedule")
            .WithDescription("Get all customer schedule");
        gr1.MapGet("{customerScheduleId:guid}", GetCustomerScheduleById)
            .RequireAuthorization()
            .WithName("Get Customer Schedule By Id")
            .WithSummary("Get Customer Schedule By Id")
            .WithDescription("Get customer schedule by id");
        gr1.MapGet("{customerScheduleId:guid}/next-schedule/availability",
                CheckIfNextCustomerScheduleIsNotScheduledYet)
            .RequireAuthorization(Constant.Role.CLINIC_STAFF)
            .WithName("Check Next Schedule Availability")
            .WithSummary("Check if next schedule is available")
            .WithDescription("Check if the next customer schedule is not scheduled yet");
        gr1.MapGet("{customerId:guid}/busy-time/{date}", GetAllCustomerBusyTime)
            .RequireAuthorization(Constant.Role.CUSTOMER);
    }


    private static async Task<IResult> GetAllCustomerBusyTime(
        ISender sender,
        [FromRoute] Guid customerId,
        [FromRoute] DateOnly date)
    {
        var result = await sender.Send(new Query.GetAllCustomerBusyTime(customerId, date));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> CheckIfNextCustomerScheduleIsNotScheduledYet(
        ISender sender,
        [FromRoute] Guid customerScheduleId)
    {
        var result = await sender.Send(
            new Query.CheckIfNextCustomerScheduleIsNotScheduledYet(customerScheduleId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> StaffCheckInCustomerSchedule(ISender sender,
        [FromRoute] string customerName,
        [FromQuery] string customerPhone,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? SearchTerm = null)
    {
        var result = await sender.Send(
            new Query.StaffCheckInCustomerScheduleQuery(customerName, customerPhone, pageIndex, pageSize, SearchTerm));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllCustomerSchedule(ISender sender,
        string SearchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllCustomerSchedule(SearchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetCustomerScheduleById(ISender sender,
        Guid customerScheduleId, bool isNextSchedule = false)
    {
        var result = await sender.Send(new Query.GetCustomerScheduleById(customerScheduleId, isNextSchedule));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}