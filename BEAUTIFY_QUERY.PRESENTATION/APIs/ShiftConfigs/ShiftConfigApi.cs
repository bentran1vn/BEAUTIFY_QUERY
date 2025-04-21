using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.ShiftConfigs;

public class ShiftConfigApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/shiftConfigs";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Shift Configs")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet("", GetAllShiftConfig)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN);
    }
    
    private static async Task<IResult> GetAllShiftConfig(ISender sender,
        HttpContext httpContext,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        
        var result = await sender.Send(new Query.GetShiftConfigQuery(new Guid(clinicId),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

}