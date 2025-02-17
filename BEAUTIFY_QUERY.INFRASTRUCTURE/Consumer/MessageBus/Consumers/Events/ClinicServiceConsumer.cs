using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ClinicServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;

public static class ClinicServiceConsumer
{
    public class ClinicServiceCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.ClinicServiceCreated>(sender, repository);
}