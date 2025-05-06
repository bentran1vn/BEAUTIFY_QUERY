using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Services;
public static class Query
{
    public record GetDiscountServicesByLiveStreamIdQuery(
        Guid ServiceId,
        Guid LiveStreamId)
        : IQuery<Response.GetDiscountServicesByLiveStreamIdResponse>;
    
    public record GetServiceByClinicIdQuery(
        Guid ClinicId)
        : IQuery<List<Response.GetAllServiceByIdResponse>>;

    public record GetServiceByCategoryIdQuery(
        Guid CategoryId)
        : IQuery<List<Response.GetAllServiceByIdResponse>>;

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
    
    public record GetDoctorClinicServicesByIdQuery(Guid ServiceId, int PageNumber,
        int PageSize): IQuery<PagedResult<Response.GetAllDoctorServiceByIdResponse>>;
    
    public record GetDoctorClinicServicesByIdQueryV2(Guid ServiceId): IQuery<Response.GetAllDoctorServiceByIdResponseV2>;

    public record GetAllServiceInGetClinicByIdQuery(
        int PageNumber,
        int PageSize)
        : IQuery<PagedResult<Response.GetAllServiceInGetClinicById>>;
}