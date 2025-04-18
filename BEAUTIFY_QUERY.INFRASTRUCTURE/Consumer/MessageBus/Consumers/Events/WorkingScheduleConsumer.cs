using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Documents;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.Consumer.Abstractions.Messages;
using MediatR;

namespace BEAUTIFY_QUERY.INFRASTRUCTURE.Consumer.MessageBus.Consumers.Events;
public static class WorkingScheduleConsumer
{
    public class WorkingScheduleCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleCreated>(sender, repository);

    public class WorkingScheduleDeletedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleDeleted>(sender, repository);

    public class WorkingScheduleUpdatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.WorkingScheduleUpdated>(sender, repository);

    public class ClinicEmptyScheduleCreatedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.ClinicEmptyScheduleCreated>(sender, repository);

    public class ClinicScheduleCapacityChangedConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.ClinicScheduleCapacityChanged>(sender, repository);

    public class DoctorScheduleRegisteredConsumer(ISender sender, IMongoRepository<EventProjection> repository)
        : Consumer<DomainEvents.DoctorScheduleRegistered>(sender, repository);
}