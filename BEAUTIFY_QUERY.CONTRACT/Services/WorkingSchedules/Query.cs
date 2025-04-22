using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
public static class Query
{
    public record GetClinicWorkingHours(Guid ClinicId)
        : IQuery<Response.GetClinicWorkingHoursResponse>;

    public record GetWorkingScheduleEachDayInMonth(DateOnly Date)
        : IQuery<Response.GetWorkingScheduleEachDayInMonthResponse>;

    public record GetWorkingScheduleDaily(DateOnly Date)
        : IQuery<IReadOnlyList<Response.GetWorkingScheduleResponseDaily>>;

    public record GetWorkingScheduleDetail(Guid Id) : IQuery<Response.GetWorkingScheduleDetailResponse>;

    public record GetWorkingSchedule(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetWorkingScheduleResponse>>;

    public record GetAllDoctorFreeTime(Guid DoctorId, Guid ClinicId, DateOnly Date)
        : IQuery<IReadOnlyList<Response.DoctorBusyTimeInADay>>;

    public record GetWorkingScheduleOfDoctorId(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.ShiftGroup>>;

    public record GetWorkingScheduleByClinicId(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetScheduleResponseForStaff>>;

    public record GetUnregisteredWorkingSchedules(
        Guid ClinicId,
        string searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetEmptyScheduleResponseWithId>>;

    public record GetDoctorAvailableTimeSlots(
        Guid serviceIdOrCustomerScheduleId,
        Guid? ClinicId,
        bool IsCustomerSchedule,
        Guid? DoctorId,
        DateOnly Date)
        : IQuery<IReadOnlyList<Response.GetEmptyScheduleResponse>>;

    public record GetSchedulesByShiftGroupId(
        Guid ShiftGroupId,
        string? SearchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetWorkingScheduleResponse_Son>>;
}