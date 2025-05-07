namespace BEAUTIFY_QUERY.CONTRACT.Services.Orders;
public static class Response
{
    public record UserQueryResponse(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string? FullName,
        DateOnly? DateOfBirth,
        string? PhoneNumber,
        decimal Balance,
        string? ProfilePicture,
        string? City,
        string? District,
        string? Ward,
        string? Address,
        string? FullAddress,
        bool IsActivated,
        DateTimeOffset JoinDate
    );
    
    public record Order(
        Guid Id,
        string CustomerName,
        string ServiceName,
        decimal? TotalAmount,
        decimal? Discount,
        decimal? DepositAmount,
        decimal? FinalAmount,
        DateTimeOffset OrderDate,
        string Status,
        string CustomerPhone,
        string CustomerEmail,
        bool IsFromLivestream,
        string? LivestreamName
    );
    
    public record OrderSystem(
        Guid Id,
        Guid BrandId,
        string BrandName,
        string BrandAddress,
        string BrandPhone,
        Guid ClinicId,
        string ClinicName,
        string ClinicAddress,
        string ClinicPhone,
        string CustomerName,
        string ServiceName,
        decimal? TotalAmount,
        decimal? Discount,
        decimal? DepositAmount,
        decimal? FinalAmount,
        DateTimeOffset OrderDate,
        string Status,
        string CustomerPhone,
        string CustomerEmail,
        bool IsFromLivestream,
        string? LivestreamName
    );

    public record OrderById(
        Guid Id,
        string CustomerName,
        string ServiceName,
        decimal? TotalAmount,
        decimal? Discount,
        decimal? DepositAmount,
        decimal? FinalAmount,
        DateTimeOffset OrderDate,
        string Status,
        string CustomerPhone,
        string CustomerEmail,
        bool IsFromLivestream,
        bool IsFinished,
        string? LivestreamName,
        string? FeedbackContent,
        int? FeedbackRating,
        List<string>? FeedbackImages,
        List<CustomerSchedule> CustomerSchedules
    );

    public record CustomerSchedule(
        Guid Id,
        Guid DoctorId,
        string DoctorName,
        string ProcedureName,
        string ProfileUrl,
        string Status,
        DateOnly? Date,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string? FeedbackContent,
        int? DoctorFeedbackRating,
        DateTimeOffset? FeedbackCreatedOnUtc);
}