using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class SubscriptionPackage : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    [Column(TypeName = "decimal(18,2)")] public required decimal Price { get; set; }
    public required int Duration { get; set; }
    public bool IsActivated { get; set; }
    public int LimitBranch { get; set; }
    public int LimitLiveStream { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PriceMoreBranch { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PriceMoreLivestream { get; set; }
    public int EnhancedViewer { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}