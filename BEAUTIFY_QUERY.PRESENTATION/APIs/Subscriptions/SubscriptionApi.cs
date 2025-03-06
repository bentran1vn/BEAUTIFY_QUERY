using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Extensions;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Subscriptions;
public class SubscriptionApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/subscriptions";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group1 = app.NewVersionedApi("Subscriptions")
            .MapGroup(BaseUrl).HasApiVersion(1);

        group1.MapGet(string.Empty, GetSubscriptions);
        group1.MapGet("{id:guid}", GetSubscriptionById);

    }

    private static async Task<IResult> GetSubscriptions(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetSubscription(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetSubscriptionById(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetSubscriptionById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}