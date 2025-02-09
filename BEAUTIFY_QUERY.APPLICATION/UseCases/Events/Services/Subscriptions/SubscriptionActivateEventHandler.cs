using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Subscriptions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Subscriptions;
internal sealed class
    SubscriptionActivateEventHandler : ICommandHandler<DomainEvents.SubscriptionStatusActivationChanged>
{
    private readonly IMongoRepository<SubscriptionProjection> _subscriptionRepository;

    public SubscriptionActivateEventHandler(IMongoRepository<SubscriptionProjection> subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(DomainEvents.SubscriptionStatusActivationChanged request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.FindOneAsync(x => x.DocumentId == request.SubscriptionId);
        if (subscription is null)
        {
            return Result.Failure(new Error("404", "Subscription not found"));
        }

        subscription.IsActivated = !subscription.IsActivated;
        await _subscriptionRepository.ReplaceOneAsync(subscription);
        return Result.Success();
    }
}