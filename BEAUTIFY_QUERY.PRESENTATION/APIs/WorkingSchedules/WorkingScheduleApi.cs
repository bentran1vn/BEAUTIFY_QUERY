using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using Microsoft.AspNetCore.Mvc;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.WorkingSchedules;
public class WorkingScheduleApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/working-schedules";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Working Schedules").MapGroup(BaseUrl).HasApiVersion(1);
        //  gr1.MapGet(string.Empty, GetWorkingSchedules)
        //      .WithSummary("Search theo Date : Date1 to Date2 or Time : Time1 to Time2|| search by DoctorName");
        //   gr1.MapGet("doctors/busy-times", GetDoctorBusyTimeInADay)
        //  .WithSummary("Get doctor's busy time slots for a specific day");
        gr1.MapGet("doctors/available-times", GetDoctorAvailableTimeSlots)
            .WithSummary("Get doctor's available time slots for booking");
        gr1.MapGet("doctors/", GetDoctorScheduleByIdV2).RequireAuthorization();
        gr1.MapGet("doctors/monthly-count", GetWorkingSchedulesEachDayInMonth)
            .WithSummary("Get doctor's busy time slots for a specific month")
            .RequireAuthorization(Constant.Role.DOCTOR);
        gr1.MapGet("doctors/daily-count", GetWorkingScheduleDaily)
            .WithSummary("Get doctor's busy time slots for a specific day")
            .RequireAuthorization(Constant.Role.DOCTOR);

        gr1.MapGet("doctors/{id:guid}", GetWorkingScheduleById)
            .WithSummary("Get doctor's busy time slots for a specific day")
            .RequireAuthorization(Constant.Role.DOCTOR);

        gr1.MapGet("clinics", GetWorkingSchedulesByClinicId)
            .WithSummary("Get working schedules by clinic ID")
            .RequireAuthorization(Constant.Role.CLINIC_STAFF);
        gr1.MapGet("{clinicId:guid}/unregistered", GetUnregisteredWorkingSchedule)
            .WithSummary("Get unregistered working schedules")
            .RequireAuthorization(Constant.Role.DOCTOR);
    }

    #region GetDoctorBusyTimeInADay

    private static async Task<IResult> GetDoctorBusyTimeInADay(
        ISender sender,
        [FromQuery] Guid doctorId,
        [FromQuery] Guid clinicId,
        [FromQuery] DateOnly date)
    {
        var query = new Query.GetAllDoctorFreeTime(doctorId, clinicId, date);
        var result = await sender.Send(query);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    #endregion

    private static async Task<IResult> GetDoctorAvailableTimeSlots(
        ISender sender,
        [FromQuery] Guid doctorId,
        [FromQuery] Guid clinicId,
        [FromQuery] DateOnly date)
    {
        var query = new Query.GetDoctorAvailableTimeSlots(doctorId, clinicId, date);
        var result = await sender.Send(query);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetUnregisteredWorkingSchedule(ISender sender,
        Guid clinicId,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetUnregisteredWorkingSchedules(clinicId, searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetWorkingSchedulesByClinicId(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetWorkingScheduleByClinicId(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetWorkingSchedulesEachDayInMonth(
        ISender sender,
        [FromQuery] DateOnly date)
    {
        var result = await sender.Send(new Query.GetWorkingScheduleEachDayInMonth(date));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetWorkingScheduleDaily(
        ISender sender,
        [FromQuery] DateOnly date)
    {
        var result = await sender.Send(new Query.GetWorkingScheduleDaily(date));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetDoctorScheduleByIdV2(ISender sender, string searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetWorkingScheduleOfDoctorId(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageNumber, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    #region GetWorkingSchedules

    private static async Task<IResult> GetWorkingScheduleById(
        ISender sender,
        [FromRoute] Guid id)
    {
        var result = await sender.Send(new Query.GetWorkingScheduleDetail(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetWorkingSchedules(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetWorkingSchedule(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    #endregion
}