using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Services;
public static class Query
{
    public record GetClinicServicesQuery(
        string? SearchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize,
        Guid? MainClinicId)
        : IQuery<PagedResult<Response.GetAllServiceResponse>>;

    public record GetClinicServicesByIdQuery(Guid ServiceId, Guid? MainClinicId)
        : IQuery<Response.GetAllServiceByIdResponse>;

    public record GetAllServiceInGetClinicByIdQuery(
        int PageNumber,
        int PageSize)
        : IQuery<PagedResult<Response.GetAllServiceInGetClinicById>>;
}