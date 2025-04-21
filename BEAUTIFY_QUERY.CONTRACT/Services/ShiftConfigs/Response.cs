namespace BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;

public class Response
{
    public record ShiftResponse(
        Guid Id,
        string Name,
        string? Note,
        TimeSpan StartTime,
        TimeSpan EndTime,
        DateTimeOffset CreatedAt);
}