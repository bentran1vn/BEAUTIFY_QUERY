using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Subscriptions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;
public static class SubscriptionConsumer
{
    public class SubscriptionCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.SubscriptionCreated>(sender, repository);

    public class SubscriptionUpdatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.SubscriptionUpdated>(sender, repository);

    public class SubscriptionDeletedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.SubscriptionDeleted>(sender, repository);

    public class SubscriptionStatusActivationChangedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.SubscriptionStatusActivationChanged>(sender, repository);
}