namespace BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
public static class Response
{
    public record StaffCheckInCustomerScheduleResponse(
        Guid Id,
        Guid orderId,
        decimal Amount,
        string CustomerName,
        string CustomerPhoneNumber,
        string ServiceName,
        string DoctorName,
        DateOnly? BookingDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Status,
        string ProcedurePriceTypeName,
        string StepIndex,
        bool IsFirstCheckIn = false);

    public record CustomerScheduleWithProceduresResponse(
        // Basic customer information
        Guid Id,
        string CustomerName,
        string ServiceName,
        DateOnly? BookingDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Status,
        string DoctorNote,

        // Current procedure details
        ProcedureDetailResponse CurrentProcedure);

    public record ProcedureDetailResponse(
        Guid Id,
        string Name,
        string StepIndex,
        DateOnly? Date);
}