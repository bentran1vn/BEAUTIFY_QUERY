namespace BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
public static class Response
{
    public record StaffCheckInCustomerScheduleResponse(
        Guid Id,
        Guid OrderId,
        decimal? ServicePrice,
        decimal? DiscountAmount,
        decimal DepositAmount,
        decimal? Amount,
        string CustomerName,
        string CustomerEmail,
        string? DoctorNote,
        string CustomerPhoneNumber,
        string ServiceName,
        string DoctorName,
        DateOnly? BookingDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Status,
        string ProcedurePriceTypeName,
        string ProcedureName,
        string StepIndex,
        bool IsFirstCheckIn = false);

    public record StaffCheckInCustomerScheduleResponse1(
        Guid Id,
        Guid OrderId,
        Guid UserId,
        decimal? ServicePrice,
        decimal? DiscountAmount,
        decimal DepositAmount,
        decimal Amount,
        string CustomerName,
        string CustomerEmail,
        string CustomerPhoneNumber,
        string ServiceName,
        string? DoctorNote,
        string DoctorName,
        DateOnly? BookingDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string Status,
        string ProcedurePriceTypeName,
        string ProcedureName,
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
        Guid DoctorId,

        // Current procedure details
        ProcedureDetailResponse CurrentProcedure);

    public record ProcedureDetailResponse(
        Guid Id,
        string Name,
        string StepIndex,
        DateOnly? Date);

    public record CustomerBusyTimeInADay(
        TimeSpan? Start,
        TimeSpan? End,
        DateOnly? Date
    );
}