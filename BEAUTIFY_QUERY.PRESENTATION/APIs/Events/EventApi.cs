using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Events;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Events;

public class EventApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/events";
    private const string Base1Url = "/api/v{version:apiVersion}/followers";
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Events")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet("", GetEvents);
        
        gr1.MapGet("{id}", GetEventById)
            .RequireAuthorization();
        
        // gr1.MapGet("{id}", () => {})
        //     .RequireAuthorization();
        
        var gr2 = app.NewVersionedApi("Followers")
            .MapGroup(Base1Url).HasApiVersion(1);
        
        gr2.MapGet("", GetFollowers)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN);

        gr2.MapGet("{id}", GetFollowersClinicId);
    }
    
    private static async Task<IResult> GetEventById(ISender sender, HttpContext httpContext, Guid id)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var roleName = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value!;
        var result = await sender.Send(new Query.GetEventByIdQuery
        {
            Id = id,
            ClinicId = roleName == Constant.Role.CLINIC_ADMIN ? new Guid(clinicId) : null,
        });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetEvents(ISender sender, HttpContext httpContext,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? searchTerm = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var roleName = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value!;
        
        if(roleName == Constant.Role.CLINIC_ADMIN)
        {
            var result = await sender.Send(new Query.GetClinicEventQuery(
                startDate,
                endDate,
                searchTerm,
                pageIndex,
                pageSize,
                new Guid(clinicId)
            ));
            return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
        } 
        else
        {
            var result = await sender.Send(new Query.GetEventQuery(
                startDate,
                endDate,
                searchTerm,
                pageIndex,
                pageSize
            ));
            return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
        }
    }
    
    private static async Task<IResult> GetFollowers(ISender sender, HttpContext httpContext,
        string? searchTerm = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var result = await sender.Send(new CONTRACT.Services.Followers.Query.GetFollowerQuery
        {
            ClinicId = new Guid(clinicId),
            SearchTerm = searchTerm,
            PageNumber = pageIndex,
            PageSize = pageSize
        });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetFollowersClinicId(ISender sender, HttpContext httpContext, Guid id)
    {
        var userId = httpContext.User.FindFirst(c => c.Type == "UserId")?.Value;
        var result = await sender.Send(new CONTRACT.Services.Followers.Query.GetFollowerClinicQuery(id, userId != null ? new Guid(userId) : null));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}