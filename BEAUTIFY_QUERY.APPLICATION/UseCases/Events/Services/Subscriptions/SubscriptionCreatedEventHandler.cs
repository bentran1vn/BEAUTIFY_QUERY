using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Subscriptions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Subscriptions;
internal sealed class SubscriptionCreatedEventHandler : ICommandHandler<DomainEvents.SubscriptionCreated>
{
    private readonly IMongoRepository<SubscriptionProjection> _subscriptionRepository;

    public SubscriptionCreatedEventHandler(IMongoRepository<SubscriptionProjection> subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(DomainEvents.SubscriptionCreated request, CancellationToken cancellationToken)
    {
        var subscription = new SubscriptionProjection
        {
            DocumentId = request.subscription.Id,
            Name = request.subscription.Name,
            Description = request.subscription.Description,
            Price = request.subscription.Price,
            Duration = request.subscription.Duration,
            IsActivated = request.subscription.IsActivated,
            IsDeleted = request.subscription.IsDeleted
        };
        await _subscriptionRepository.InsertOneAsync(subscription);
        return Result.Success();
    }
}