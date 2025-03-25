namespace BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
public static class Response
{
    public record StaffCheckInCustomerScheduleResponse(
        Guid Id,
        string CustomerName,
        string CustomerPhoneNumber,
        string ServiceName,
        string DoctorName,
        DateOnly BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Status,
        string ProcedurePriceTypeName);
}