using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Role : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(50)] public required string Name { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}