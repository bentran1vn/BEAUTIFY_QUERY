using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Order : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public virtual User? Customer { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")] public required decimal DepositAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? FinalAmount { get; set; }
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public Guid? ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public Guid? LivestreamRoomId { get; set; }
    public virtual LivestreamRoom? LivestreamRoom { get; set; }

    [MaxLength(50)] public string? Status { get; set; }
    public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = [];
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; } = [];
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}