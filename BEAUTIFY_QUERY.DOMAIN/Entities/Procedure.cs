using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Procedure : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(100)] public required string Name { get; set; }
    public string Description { get; set; } = default!;
    public int StepIndex { get; set; }


    public Guid? ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; } = [];
    public virtual ICollection<ProcedureMedia> ProcedureMedias { get; set; } = [];
    public virtual ICollection<ProcedurePriceTypes> ProcedurePriceTypes { get; set; } = [];

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}