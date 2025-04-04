using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;
public class CustomerScheduleConsumer
{
    public class CustomerScheduleCreatedEvent(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CustomerScheduleCreated>(sender, repository);

    public class CustomerScheduleDeletedEvent(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CustomerScheduleDeleted>(sender, repository);

    public class CustomerScheduleUpdateAfterPaymentCompletedEvent(
        ISender sender,
        IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CustomerScheduleUpdateAfterPaymentCompleted>(sender, repository);

    public class CustomerScheduleUpdatedDoctorNoteEvent(
        ISender sender,
        IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CustomerScheduleUpdatedDoctorNote>(sender, repository);

    public class CustomerScheduleUpdateDateAndTimeEvent(
        ISender sender,
        IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.CustomerScheduleUpdateDateAndTimeAndStatus>(sender, repository);
}