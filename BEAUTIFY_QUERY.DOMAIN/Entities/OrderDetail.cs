using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class OrderDetail : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public Guid ProcedurePriceTypeId { get; set; }
    public virtual ProcedurePriceTypes? ProcedurePriceType { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    // public Guid? FeedbackId { get; set; }
    public virtual Feedback? Feedback { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}