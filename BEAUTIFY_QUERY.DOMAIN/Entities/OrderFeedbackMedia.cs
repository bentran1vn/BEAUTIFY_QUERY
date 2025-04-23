namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class OrderFeedbackMedia: AggregateRoot<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = default!;
    
    public string MediaUrl { get; set; } 
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}