using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;

public class ServiceProcedureConsumer
{
    public class ProcedureCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.ProcedureCreated>(sender, repository);
}