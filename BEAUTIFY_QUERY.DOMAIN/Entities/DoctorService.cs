namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class DoctorService : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual Staff? Doctor { get; set; }
    public Guid ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}