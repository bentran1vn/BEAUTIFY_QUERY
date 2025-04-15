using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Feedback;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;

public class FeedbackConsumer
{
    public class CreateFeedbackConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CreateFeedback>(sender, repository);
}