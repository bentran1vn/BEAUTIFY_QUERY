namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class Event : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    
    public virtual ICollection<LivestreamRoom>? LivestreamRoom { get; set; }
    
    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}