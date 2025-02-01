using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.SubscriptionPackage)]
public class SubscriptionProjection : Document
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActivated { get; set; }
    
}