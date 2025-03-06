using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using MediatR;

namespace BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
public static class Query
{
    public record GetDoctorCertificateByDoctorId(Guid DoctorId)
        : IQuery<IReadOnlyList<Response.GetDoctorCertificateByResponse>>;

    public record GetDoctorCertificateById(Guid Id) : IQuery<Response.GetDoctorCertificateByResponse>;

    public record GetAllDoctorCertificates(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetDoctorCertificateByResponse>>;
}