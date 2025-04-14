using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Dashboard;

public class DashboardApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/dashboards";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Dashboards")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);
        
        gr1.MapGet("clinics", GetTotalInformation).RequireAuthorization();
        gr1.MapGet("clinics/datetime", GetDaytimeInformation).RequireAuthorization();
        gr1.MapGet("systems", GetDaytimeInformation).RequireAuthorization();
        gr1.MapGet("systems/datetime", GetDaytimeInformation).RequireAuthorization();
    }
    
    private static async Task<IResult> GetTotalInformation(
        ISender sender,
        HttpContext httpContext)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var roleName = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value!;
        var result = await sender.Send(new Query.GetTotalInformationQuery(roleName, new Guid(clinicId)));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
    
    private static async Task<IResult> GetDaytimeInformation(
        ISender sender,
        HttpContext httpContext,
        DateOnly? startDate,
        DateOnly? endDate,
        bool? isDisplayWeek,
        DateOnly? date)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var roleName = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value!;
        var result = await sender.Send(new Query.GetDaytimeInformationQuery(
            roleName,
            new Guid(clinicId),
            startDate,
            endDate,
            isDisplayWeek,
            date));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
}