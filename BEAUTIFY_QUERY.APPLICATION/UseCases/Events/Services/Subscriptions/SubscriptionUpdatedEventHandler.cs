using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Subscriptions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Subscriptions;
internal sealed class SubscriptionUpdatedEventHandler : ICommandHandler<DomainEvents.SubscriptionUpdated>
{
    private readonly IMongoRepository<SubscriptionProjection> _subscriptionRepository;

    public SubscriptionUpdatedEventHandler(IMongoRepository<SubscriptionProjection> subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(DomainEvents.SubscriptionUpdated request, CancellationToken cancellationToken)
    {
        var currentSubscription = await _subscriptionRepository.FindOneAsync(x=>x.DocumentId == request.subscription.Id);
        if (currentSubscription is null)
        {
            return Result.Failure(new Error("404", "Subscription not found"));
        }

        currentSubscription.Name = request.subscription.Name;
        currentSubscription.Description = request.subscription.Description;
        currentSubscription.Price = request.subscription.Price;
        currentSubscription.Duration = request.subscription.Duration;
        currentSubscription.IsActivated = request.subscription.IsActivated;
        currentSubscription.IsDeleted = request.subscription.IsDeleted;
        await _subscriptionRepository.ReplaceOneAsync(currentSubscription);
        return Result.Success();
    }
}