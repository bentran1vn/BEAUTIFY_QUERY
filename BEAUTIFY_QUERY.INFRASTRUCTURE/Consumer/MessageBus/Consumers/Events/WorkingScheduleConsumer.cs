using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using MediatR;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;
public static class WorkingScheduleConsumer
{
    public class WorkingScheduleCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleCreated>(sender, repository);

    public class WorkingScheduleDeletedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleDeleted>(sender, repository);

    public class WorkingScheduleUpdatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleUpdated>(sender, repository);
}