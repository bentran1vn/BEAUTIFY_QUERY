namespace BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;

public static class Query
{
    public record GetTotalInformationQuery(
        string RoleName,
        Guid ClinicId)
        : IQuery<Responses.GetTotalInformationResponse>;
    
    public record GetDaytimeInformationQuery(
        string RoleName,
        Guid ClinicId,
        DateOnly? StartDate,
        DateOnly? EndDate,
        bool? IsDisplayWeek,
        DateOnly? Date)
        : IQuery<Responses.GetDaytimeInformationResponse>;
    
    public record GetSystemTotalInformationQuery(
        string RoleName)
        : IQuery<Responses.GetSystemTotalInformationResponse>;
    
    public record GetSystemDaytimeInformationQuery(
        string RoleName,
        DateOnly? StartDate,
        DateOnly? EndDate,
        bool? IsDisplayWeek,
        DateOnly? Date)
        : IQuery<Responses.GetSystemDaytimeInformationResponse>;
}