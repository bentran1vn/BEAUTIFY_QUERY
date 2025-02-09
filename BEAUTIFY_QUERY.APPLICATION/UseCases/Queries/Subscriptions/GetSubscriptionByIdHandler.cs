using System.Globalization;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Subscriptions;
public class GetSubscriptionByIdHandler : IQueryHandler<Query.GetSubscriptionById, Response.GetSubscriptionResponse>
{
    private readonly IMongoRepository<SubscriptionProjection> _mongoRepository;

    public GetSubscriptionByIdHandler(IMongoRepository<SubscriptionProjection> mongoRepository)
    {
        _mongoRepository = mongoRepository;
    }

    public async Task<Result<Response.GetSubscriptionResponse>> Handle(Query.GetSubscriptionById request,
        CancellationToken cancellationToken)
    {
        var subscription = await _mongoRepository.FindOneAsync(x => x.DocumentId == request.Id);
        if (subscription is null)
            return Result.Failure<Response.GetSubscriptionResponse>(new Error("404", "Subscription not found"));
        var response = new Response.GetSubscriptionResponse
        {
            DocumentId = subscription.DocumentId,
            Name = subscription.Name,
            Description = subscription.Description,
            Price = subscription.Price.ToString("C0", new CultureInfo("vi-VN")),
            Duration = subscription.Duration
        };
        return Result.Success(response);
    }
}