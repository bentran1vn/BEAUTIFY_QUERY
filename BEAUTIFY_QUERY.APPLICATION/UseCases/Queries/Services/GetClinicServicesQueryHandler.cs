using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetClinicServicesQueryHandler: IQueryHandler<Query.GetClinicServicesQuery, PagedResult<Response.GetAllServiceResponse>>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public GetClinicServicesQueryHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result<PagedResult<Response.GetAllServiceResponse>>> Handle(Query.GetClinicServicesQuery request, CancellationToken cancellationToken)
    {
        // 1. Trim and store the search term
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        
        var query = _clinicServiceRepository.AsQueryable();
        
        // 3. If a search term was provided, filter further
        if (request.MainClinicId.HasValue)
        {
            query = query.Where(x => x.Clinic.Any(
                c => c.Id == request.MainClinicId.Value ||
                     c.ParentId == request.MainClinicId.Value)
            );
        }
        
        // 3. If a search term was provided, filter further
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm) || x.Description.Contains(searchTerm));
        }
        
        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
        
        // 5. Apply pagination
        var services = await PagedResult<ClinicServiceProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );
        
        var mapList = services.Items.Select(x => new Response.GetAllServiceResponse(
            x.DocumentId, x.Name, x.MaxPrice, x.MinPrice, (x.DiscountPercent * 100).ToString(),
            x.DiscountMaxPrice, x.DiscountMinPrice,
            x.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
            x.Clinic.Select(y => new Response.Clinic(y.Id, y.Name, y.Email,
                y.Address, y.PhoneNumber, y.ProfilePictureUrl, y.IsParent, y.ParentId)).ToList(),
            new Response.Category(x.Category.Id, x.Category.Name, x.Category.Description))
        ).ToList();
        
        var result = new PagedResult<Response.GetAllServiceResponse>(mapList, services.PageIndex, services.PageSize, services.TotalCount);
        
        return Result.Success(result);
    }
    
    private static Expression<Func<ClinicServiceProjection, object>> GetSortProperty(Query.GetClinicServicesQuery request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            "price" => projection => projection.DiscountMaxPrice,
            _ => projection => projection.CreatedOnUtc
        };
    }
}