using System.Collections.ObjectModel;

namespace BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
public static class Query
{
    public record StaffCheckInCustomerScheduleQuery(
        string CustomerName,
        string? CustomerPhone)
        : IQuery<List<Response.StaffCheckInCustomerScheduleResponse>>;
}