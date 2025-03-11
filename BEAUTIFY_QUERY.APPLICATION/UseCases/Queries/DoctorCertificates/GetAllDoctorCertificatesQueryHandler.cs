using System.Linq.Expressions;
using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;
public class GetAllDoctorCertificatesQueryHandler(IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetAllDoctorCertificates,
        PagedResult<Response.GetDoctorCertificateByResponse>>
{
    public async Task<Result<PagedResult<Response.GetDoctorCertificateByResponse>>> Handle(
        Query.GetAllDoctorCertificates request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        var query = doctorCertificateRepository.FindAll();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower(); // Convert search term to lowercase

            query = query.Where(x =>
                x.CertificateName.ToLower().Contains(lowerSearchTerm)
                || x.Doctor.FirstName.ToLower().Contains(lowerSearchTerm)
                || x.Doctor.LastName.ToLower().Contains(lowerSearchTerm)
            );
        }


        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        // Fetch paged result
        var doctorCertificates = await PagedResult<DoctorCertificate>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );


        var mappedCertificates = doctorCertificates.Items.Select(dc => new Response.GetDoctorCertificateByResponse
        {
            Id = dc.Id,
            CertificateName = dc.CertificateName,
            CertificateUrl = dc.CertificateUrl,
            ExpiryDate = dc.ExpiryDate,
            Note = dc.Note,
            DoctorName = $"{dc.Doctor.FirstName} {dc.Doctor.LastName}"
        }).ToList();


        var result = new PagedResult<Response.GetDoctorCertificateByResponse>(
            mappedCertificates,
            doctorCertificates.PageIndex,
            doctorCertificates.PageSize,
            doctorCertificates.TotalCount
        );

        return Result.Success(result);
    }


    private static Expression<Func<DoctorCertificate, object>> GetSortProperty(Query.GetAllDoctorCertificates request)
    {
        return request.SortColumn switch
        {
            "DoctorName" => x => x.Doctor.FirstName,
            _ => x => x.CertificateName
        };
    }
}