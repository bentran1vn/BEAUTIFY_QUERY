using System.Collections.ObjectModel;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
public static class Query
{
    public record StaffCheckInCustomerScheduleQuery(
        string CustomerName,
        string? CustomerPhone)
        : IQuery<List<Response.StaffCheckInCustomerScheduleResponse>>;

    public record GetAllCustomerSchedule(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.StaffCheckInCustomerScheduleResponse1>>;

    public record GetCustomerScheduleById(Guid Id, bool IsNext = false)
        : IQuery<Response.CustomerScheduleWithProceduresResponse>;


    public record CheckIfNextCustomerScheduleIsNotScheduledYet(
        Guid CustomerScheduleId) : IQuery<string>;
}