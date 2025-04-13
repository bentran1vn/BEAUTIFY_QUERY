using Query = BEAUTIFY_QUERY.CONTRACT.Services.Services.Query;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Services;
public class ServiceApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/services";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Services")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet(string.Empty, GetAllServices);
        gr1.MapGet("{id}", GetServicesById);
        //gr1.MapGet("clinic", GetServicesByClinicId);
        gr1.MapGet("clinics/{clinicId:guid}", GetServicesByClinicIdForCustomer);
        gr1.MapGet("categories/{categoryId:guid}", GetServicesByCategoryId);
    }


    private static async Task<IResult> GetServicesByCategoryId(ISender sender, Guid categoryId)
    {
        var result = await sender.Send(new Query.GetServiceByCategoryIdQuery(categoryId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetServicesByClinicIdForCustomer(
        ISender sender, Guid clinicId)
    {
        var result = await sender.Send(new Query.GetServiceByClinicIdQuery(clinicId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllServices(
        ISender sender, HttpContext httpContext, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var mainClinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value;

        var result = await sender.Send(new Query.GetClinicServicesQuery(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize, !string.IsNullOrEmpty(mainClinicId) ? new Guid(mainClinicId) : null));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetServicesById(
        ISender sender, HttpContext httpContext, Guid id)
    {
        var mainClinicId = httpContext.User.FindFirst(c => c.Type == "ClinicId")?.Value;

        var result = await sender.Send(new Query.GetClinicServicesByIdQuery(id,
            !string.IsNullOrEmpty(mainClinicId) ? new Guid(mainClinicId) : null));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetServicesByClinicId(
        ISender sender, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllServiceInGetClinicByIdQuery(pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}