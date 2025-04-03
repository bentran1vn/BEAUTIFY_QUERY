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
        gr1.MapGet("{id}", GetClinicDetail).RequireAuthorization(Constant.Role.CLINIC_ADMIN);
        gr1.MapGet("application", GetAllApplyRequest);
        gr1.MapGet("application/{id}", GetDetailApplyRequest);
        gr1.MapGet("{clinicId:guid}/employees", GetAllAccountOfEmployee);
        gr1.MapGet("{clinicId:guid}/employees/{employeeId:guid}", GetDetailAccountOfEmployee);
    }

    private static async Task<IResult> GetAllClinics(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetClinicsQuery(searchTerm,
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

    private static async Task<IResult> GetDetailApplyRequest(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetDetailApplyRequestQuery(id));
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