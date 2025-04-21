namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class ShiftConfig: AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Note { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; } = default!;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}