namespace BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;

public static class Query
{
    public record GetTotalInformationQuery(
        string RoleName,
        Guid ClinicId)
        : IQuery<Response.GetTotalInformationResponse>;
    
    public record GetDaytimeInformationQuery(
        string RoleName,
        Guid ClinicId,
        DateOnly? StartDate,
        DateOnly? EndDate,
        bool? IsDisplayWeek,
        DateOnly? Date)
        : IQuery<Response.GetDaytimeInformationResponse>;
}