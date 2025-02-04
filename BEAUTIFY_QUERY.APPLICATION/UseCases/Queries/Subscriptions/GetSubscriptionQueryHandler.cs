using System.Globalization;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Subscriptions;
internal sealed class
    GetSubscriptionQueryHandler : IQueryHandler<Query.GetSubscription, PagedResult<Response.GetSubscriptionResponse>>
{
    private readonly IMapper _mapper;
    private readonly IMongoRepository<SubscriptionProjection> _mongoRepository;

    public GetSubscriptionQueryHandler(IMapper mapper, IMongoRepository<SubscriptionProjection> mongoRepository)
    {
        _mapper = mapper;
        _mongoRepository = mongoRepository;
    }

    public async Task<Result<PagedResult<Response.GetSubscriptionResponse>>> Handle(Query.GetSubscription request,
        CancellationToken cancellationToken)
    {
        // 1. Trim and store the search term
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // 2. Base query: only return non-deleted records
        var query = _mongoRepository.AsQueryable(x => !x.IsDeleted);

        // 3. If a search term was provided, filter further
        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Try parsing the searchTerm as a decimal
            var isDecimalSearch = decimal.TryParse(searchTerm, out var decimalValue);

            // Filter by name (case-insensitive) OR by price if it's a valid decimal
            query = query.Where(x =>
                x.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                || (isDecimalSearch && x.Price == decimalValue));
        }

        // 4. Sort by the requested sort column
        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        // 5. Apply pagination
        var subscriptions = await PagedResult<SubscriptionProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );
        
        

        // 6. Map to the response DTO and return
        var result = _mapper.Map<PagedResult<Response.GetSubscriptionResponse>>(subscriptions);
        //format the price into VND currency
        foreach (var item in result.Items)
        {
            if (decimal.TryParse(item.Price, out var price))
            {
                item.Price = price.ToString("C0", new CultureInfo("vi-VN"));
            }
        }
        
        return Result.Success(result);
    }

    private static Expression<Func<SubscriptionProjection, object>> GetSortProperty(Query.GetSubscription request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            "price" => projection => projection.Price,
            _ => projection => projection.CreatedOnUtc
        };
    }

}