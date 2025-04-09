﻿using System.Linq.Expressions;
using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Subscriptions;
internal sealed class
    GetSubscriptionQueryHandler(IMapper mapper, IRepositoryBase<SubscriptionPackage, Guid> mongoRepository)
    : IQueryHandler<Query.GetSubscription, PagedResult<Response.GetSubscriptionResponse>>
{
    public async Task<Result<PagedResult<Response.GetSubscriptionResponse>>> Handle(Query.GetSubscription request,
        CancellationToken cancellationToken)
    {
        // 1. Trim and store the search term
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // 2. Base query: only return non-deleted records
        var query = mongoRepository.FindAll(x => !x.IsDeleted);

        // 3. If a search term was provided, filter further
        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Try parsing the searchTerm as a decimal
            var isDecimalSearch = decimal.TryParse(searchTerm, out var decimalValue);

            // Filter by name (case-insensitive) OR by price if it's a valid decimal
            // Use EF.Functions.Like for case-insensitive search instead of Contains with StringComparison
            query = query.Where(x =>
                EF.Functions.Like(x.Name, $"%{searchTerm}%")
                || (isDecimalSearch && x.Price == decimalValue));
        }

        // 4. Sort by the requested sort column
        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        // 5. Apply pagination
        var subscriptions = await PagedResult<SubscriptionPackage>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );


        // 6. Map to the response DTO and return
        var result = mapper.Map<PagedResult<Response.GetSubscriptionResponse>>(subscriptions);
        return Result.Success(result);
    }

    private static Expression<Func<SubscriptionPackage, object>> GetSortProperty(Query.GetSubscription request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            "price" => projection => projection.Price,
            _ => projection => projection.CreatedOnUtc
        };
    }
}