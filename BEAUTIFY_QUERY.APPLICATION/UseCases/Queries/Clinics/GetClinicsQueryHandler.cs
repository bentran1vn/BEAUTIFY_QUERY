using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetClinicsQueryHandler : IQueryHandler<Query.GetClinicsQuery, PagedResult<Response.GetClinics>>
{
    private readonly IRepositoryBase<DOMAIN.Entities.Clinic, Guid> _clinicRepository;

    public GetClinicsQueryHandler(IRepositoryBase<DOMAIN.Entities.Clinic, Guid> clinicRepository)
    {
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<PagedResult<Response.GetClinics>>> Handle(Query.GetClinicsQuery request, CancellationToken cancellationToken)
    {
        var clinicsQuery = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? _clinicRepository.FindAll(x => !x.IsDeleted)
            : _clinicRepository.FindAll(
                x => (x.Name.ToLower().Contains(request.SearchTerm.ToLower())
                      || x.Email.ToLower().Contains(request.SearchTerm.ToLower())
                      || x.Address.ToLower().Contains(request.SearchTerm.ToLower()))
                     && !x.IsDeleted
            );
        
        clinicsQuery = request.SortOrder == SortOrder.Descending
            ? clinicsQuery.OrderByDescending(GetSortProperty(request))
            : clinicsQuery.OrderBy(GetSortProperty(request));
        
        var clinics = await PagedResult<DOMAIN.Entities.Clinic>.CreateAsync(clinicsQuery,
            request.PageIndex,
            request.PageSize);
        
        var result = new PagedResult<Response.GetClinics>(clinics.Items
            .Select(x => 
                new Response.GetClinics(x.Id, x.Name, x.Email, x.Address,
                    x.TotalBranches ?? 0, x.IsActivated)).ToList(),
            clinics.PageIndex, clinics.PageSize, clinics.TotalCount);
        
        return Result.Success(result);
    }
    
    private static Expression<Func<DOMAIN.Entities.Clinic, object>> GetSortProperty(Query.GetClinicsQuery request)
        => request.SortColumn?.ToLower() switch
        {
            "name" => clinics => clinics.Name,
            "totalBranches" => clinics => clinics.TotalBranches,
            _ => clinics => clinics.CreatedOnUtc
            //_ => product => product.CreatedDate // Default Sort Descending on CreatedDate column
        };
    
}