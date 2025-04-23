using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class OrderFeedback : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    [MaxLength(5000)] public string? Content { get; set; }
    public int Rating { get; set; }
    public virtual ICollection<OrderFeedbackMedia>? OrderFeedbackMedias { get; set; } = [];
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}