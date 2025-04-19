using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetClinicsQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository
)
    : IQueryHandler<Query.GetClinicsQuery, PagedResult<Response.GetClinics>>
{
    public async Task<Result<PagedResult<Response.GetClinics>>> Handle(Query.GetClinicsQuery request,
        CancellationToken cancellationToken)
    {
        var clinicsQuery = clinicRepository.FindAll(x => true);

        clinicsQuery = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? clinicsQuery
            : clinicRepository.FindAll(x => (x.Name.ToLower().Contains(request.SearchTerm.ToLower())
                                             || x.Email.ToLower().Contains(request.SearchTerm.ToLower())
                                             || x.Address.ToLower().Contains(request.SearchTerm.ToLower()))
            );

        if (!(request.Role is Constant.Role.CLINIC_ADMIN || request.Role is Constant.Role.CLINIC_STAFF))
        {
            clinicsQuery = clinicsQuery
                .Where(x => x.IsActivated && x.IsParent.Value && !x.IsDeleted);
        }

        clinicsQuery = request.SortOrder == SortOrder.Descending
            ? clinicsQuery.OrderByDescending(GetSortProperty(request))
            : clinicsQuery.OrderBy(GetSortProperty(request));

        var clinics = await PagedResult<Clinic>.CreateAsync(clinicsQuery,
            request.PageIndex,
            request.PageSize);

        var result = new PagedResult<Response.GetClinics>(clinics.Items
                .Select(x =>
                    new Response.GetClinics(x.Id, x.Name, x.Email, x.FullAddress,
                        x.TotalBranches ?? 0, x.ProfilePictureUrl, x.IsActivated)).ToList(),
            clinics.PageIndex, clinics.PageSize, clinics.TotalCount);

        return Result.Success(result);
    }

    private static Expression<Func<Clinic, object>> GetSortProperty(Query.GetClinicsQuery request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => clinics => clinics.Name,
            "totalBranches" => clinics => clinics.TotalBranches,
            _ => clinics => clinics.CreatedOnUtc
            //_ => product => product.CreatedDate // Default Sort Descending on CreatedDate column
        };
    }
}