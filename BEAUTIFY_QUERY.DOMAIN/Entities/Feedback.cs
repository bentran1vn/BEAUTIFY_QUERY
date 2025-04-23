using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Feedback : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid CustomerScheduleId { get; set; }
    [MaxLength(500)] public string? Content { get; set; }
    public int Rating { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}