namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class ProcedurePriceType : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    public Guid ProcedureId { get; set; } = default!;
    public virtual Procedure Procedure { get; set; } = default!;
    public int Duration { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}