using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Subscriptions;
public class GetSubscriptionByIdHandler(IRepositoryBase<SubscriptionPackage, Guid> mongoRepository)
    : IQueryHandler<Query.GetSubscriptionById, Response.GetSubscriptionResponse>
{
    public async Task<Result<Response.GetSubscriptionResponse>> Handle(Query.GetSubscriptionById request,
        CancellationToken cancellationToken)
    {
        var subscription = await mongoRepository.FindByIdAsync(request.Id, cancellationToken);
        if (subscription is null)
            return Result.Failure<Response.GetSubscriptionResponse>(new Error("404", "Subscription not found"));
        var response = new Response.GetSubscriptionResponse
        {
            Id = subscription.Id,
            Name = subscription.Name,
            Description = subscription.Description,
            Price = subscription.Price,
            Duration = subscription.Duration,
            LimitBranch = subscription.LimitBranch,
            LimitLiveStream = subscription.LimitLiveStream,
            EnhancedViewer = subscription.EnhancedViewer,
            IsActivated = subscription.IsActivated,
        };
        return Result.Success(response);
    }
}