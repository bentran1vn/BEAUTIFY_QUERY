using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class ProcedurePriceTypes : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; set; } = default!;

    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }

    public required int Duration { get; set; }

    public required bool IsDefault { get; set; }
    public Guid ProcedureId { get; set; } = default!;
    public virtual Procedure Procedure { get; set; } = default!;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}