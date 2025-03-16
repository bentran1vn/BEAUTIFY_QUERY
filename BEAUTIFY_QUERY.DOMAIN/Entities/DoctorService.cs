using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.DoctorServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class DoctorService : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual Staff? Doctor { get; set; }
    public Guid ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }

    public void RaiseDoctorServiceCreatedEvent(List<EntityEvent.DoctorServiceEntity> services)
    {
        RaiseDomainEvent(new DomainEvents.DoctorServiceCreated(Guid.NewGuid(), services));
    }

    public void RaiseDoctorServiceDeletedEvent(Guid ServiceIdRemove, List<Guid> DoctorServiceIds)
    {
        RaiseDomainEvent(new DomainEvents.DoctorServiceDeleted(Guid.NewGuid(), ServiceIdRemove, DoctorServiceIds));
    }
}