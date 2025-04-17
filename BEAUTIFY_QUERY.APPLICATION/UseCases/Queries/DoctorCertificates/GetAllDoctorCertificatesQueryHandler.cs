using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;
public class GetAllDoctorCertificatesQueryHandler(
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetAllDoctorCertificates, PagedResult<Response.GetDoctorCertificateByResponse>>
{
    public async Task<Result<PagedResult<Response.GetDoctorCertificateByResponse>>> Handle(
        Query.GetAllDoctorCertificates request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        var query = doctorCertificateRepository.FindAll();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm)) query = ApplySearchFilter(query, searchTerm);

        // Apply sorting
        query = ApplySorting(query, request);

        // Fetch paged result
        var pagedCertificates = await PagedResult<DoctorCertificate>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );

        // Map to response
        var mappedCertificates = pagedCertificates.Items.Select(MapToResponse).ToList();

        var result = new PagedResult<Response.GetDoctorCertificateByResponse>(
            mappedCertificates,
            pagedCertificates.PageIndex,
            pagedCertificates.PageSize,
            pagedCertificates.TotalCount
        );

        return Result.Success(result);
    }

    private static IQueryable<DoctorCertificate> ApplySearchFilter(
        IQueryable<DoctorCertificate> query, string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return query.Where(x =>
            x.CertificateName.Contains(lowerSearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
            x.Doctor.FirstName.Contains(lowerSearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
            x.Doctor.LastName.Contains(lowerSearchTerm, StringComparison.CurrentCultureIgnoreCase)
        );
    }

    private static IQueryable<DoctorCertificate> ApplySorting(
        IQueryable<DoctorCertificate> query, Query.GetAllDoctorCertificates request)
    {
        var sortProperty = GetSortProperty(request);
        return request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(sortProperty)
            : query.OrderBy(sortProperty);
    }

    private static Expression<Func<DoctorCertificate, object>> GetSortProperty(
        Query.GetAllDoctorCertificates request)
    {
        return request.SortColumn switch
        {
            "doctor_name" => x => x.Doctor.FirstName,
            _ => x => x.CreatedOnUtc
        };
    }

    private static Response.GetDoctorCertificateByResponse MapToResponse(DoctorCertificate certificate)
    {
        return new Response.GetDoctorCertificateByResponse
        {
            Id = certificate.Id,
            CertificateName = certificate.CertificateName,
            CertificateUrl = certificate.CertificateUrl,
            ExpiryDate = certificate.ExpiryDate,
            Note = certificate.Note,
            DoctorName = $"{certificate.Doctor.FirstName} {certificate.Doctor.LastName}"
        };
    }
}