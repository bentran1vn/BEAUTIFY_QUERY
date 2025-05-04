using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.DoctorCertificates;
public class DoctorCertificateApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/doctor-certificates";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Doctor Certificates")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        // Get all certificates for a specific doctor
        gr1.MapGet("doctors/{doctorId:guid}/certificates", GetCertificatesByDoctorId).RequireAuthorization();

        // Get a specific certificate by its ID
        gr1.MapGet("{id:guid}", GetCertificateById).RequireAuthorization();

        // Get all certificates (with filtering, sorting, and pagination)
        gr1.MapGet(string.Empty, GetAllCertificates).RequireAuthorization();
    }

    private static async Task<IResult> GetCertificatesByDoctorId(
        ISender sender,
        Guid doctorId)
    {
        var result = await sender.Send(new Query.GetDoctorCertificateByDoctorId(doctorId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetCertificateById(
        ISender sender,
        Guid id)
    {
        var result = await sender.Send(new Query.GetDoctorCertificateById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllCertificates(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllDoctorCertificates(
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageNumber,
            pageSize
        ));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}