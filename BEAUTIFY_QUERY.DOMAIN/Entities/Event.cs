namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class Event : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public TimeOnly? StartDate { get; set; }
    public TimeOnly? EndDate { get; set; }
    public DateOnly? Date { get; set; }
    
    public Guid? LivestreamRoomId { get; set; }
    public virtual LivestreamRoom? LivestreamRoom { get; set; }
    
    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}