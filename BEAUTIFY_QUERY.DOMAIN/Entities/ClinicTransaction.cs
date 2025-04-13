using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class ClinicTransaction : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(50)] public string? Status { get; set; }
    [MaxLength(50)] public string? PaymentMethod { get; set; }


    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}