using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Clinics;
public class ClinicApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/clinics";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Clinics")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet(string.Empty, GetAllClinics);
        
        gr1.MapGet("{id}", GetClinicDetail); 
        
        gr1.MapGet("application", GetAllApplyRequest)
            .RequireAuthorization();
        
        gr1.MapGet("application/clinic", GetAllApplyClinicRequest)
            .RequireAuthorization();
        
        gr1.MapGet("application/{id}", GetDetailApplyRequest)
            .RequireAuthorization();
        
        gr1.MapGet("application/{id}/clinic", GetDetailBranchApplyRequest)
            .RequireAuthorization();
        
        gr1.MapGet("application/me", GetMyApplyRequest)
            .RequireAuthorization();
        
        gr1.MapGet("{clinicId:guid}/employees", GetAllAccountOfEmployee);
        
        gr1.MapGet("{clinicId:guid}/employees/{employeeId:guid}", GetDetailAccountOfEmployee);
        
        gr1.MapGet("branches", GetClinicBranches)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN)
            .WithName("Get Clinic Branches")
            .WithSummary("Get all branches for the current clinic admin");
        
        gr1.MapGet("branches/{id:guid}", GetClinicBranchById)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN);
        
        gr1.MapGet("sub-clinics/{id:guid}", GetSubClinicById)
            .RequireAuthorization(Constant.Role.CLINIC_STAFF)
            .WithName("Get Sub Clinic By Id")
            .WithSummary("Get a specific sub-clinic by ID")
            .WithDescription(
                "Retrieves details of a specific sub-clinic that belongs to the current clinic admin's main clinic");
    }

    private static async Task<IResult> GetSubClinicById(
        ISender sender,
        Guid id)
    {
        var result = await sender.Send(new Query.GetSubClinicByIdQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetClinicBranchById(
        ISender sender,
        Guid id)
    {
        var result = await sender.Send(new Query.GetClinicByIdQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetClinicBranches(ISender sender)
    {
        var result = await sender.Send(new Query.GetClinicBranchesQuery());
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllClinics(
        ISender sender,
        HttpContext httpContext,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var role = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value;

        var result = await sender.Send(new Query.GetClinicsQuery(searchTerm, role,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetClinicDetail(ISender sender, Guid id, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetClinicDetailQuery(id, searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllApplyRequest(ISender sender, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllApplyRequestQuery(pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetAllApplyClinicRequest(ISender sender ,
        HttpContext httpContext, string? searchTerm = null,
        int pageIndex = 1, int pageSize = 10)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value;
        var result = await sender.Send(new Query.GetAllApplyBranchRequestQuery(clinicId == null? null : new Guid(clinicId),
            searchTerm, pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetDetailApplyRequest(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetDetailApplyRequestQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetDetailBranchApplyRequest(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetDetailBranchApplyRequestQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetMyApplyRequest(ISender sender, HttpContext httpContext)
    {
        var clinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value!;
        var roleName = httpContext.User.FindFirst(c => c.Type == "RoleName")?.Value!;
        var result = await sender.Send(new Query.GetClinicMyQuery(new Guid(clinicId), roleName));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllAccountOfEmployee(ISender sender, Guid clinicId,
        Query.Roles? role, string? searchTerm = null,
        int pageIndex = 1, int pageSize = 10)
    {
        var result =
            await sender.Send(new Query.GetAllAccountOfEmployeeQuery(clinicId, role, searchTerm, pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetDetailAccountOfEmployee(ISender sender, Guid clinicId,
        Guid employeeId)
    {
        var result = await sender.Send(new Query.GetDetailAccountOfEmployeeQuery(clinicId, employeeId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}