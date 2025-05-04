namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class Follower: AggregateRoot<Guid>, IAuditableEntity
{
    public Guid ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; } = null!;
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}