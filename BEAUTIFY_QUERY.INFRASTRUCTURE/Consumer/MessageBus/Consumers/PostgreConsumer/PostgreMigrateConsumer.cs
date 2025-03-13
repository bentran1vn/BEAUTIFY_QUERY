using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CommandConverts;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.PostgreConsumer;
public static class PostgreConsumer
{
    // public class SubscriptionCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
    //     : Consumer<DomainEvents.SubscriptionCreated>(sender, repository);

    public class PostgreMigrateConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.PostgreMigrate>(sender, repository);
}