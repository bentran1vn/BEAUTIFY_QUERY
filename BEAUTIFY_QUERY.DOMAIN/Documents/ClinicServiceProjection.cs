using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;

[BsonCollection(TableNames.Service)]
public class ClinicServiceProjection: Document
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<string> CoverImage { get; set; }
    public ICollection<string> DescriptionImage { get; set; }
    public decimal Price { get; set; }
    public Category Category { get; set; }
    public Clinic Clinic { get; set; }
    
    public ICollection<Procedure> Procedures { get; set; } = Array.Empty<Procedure>();
}

public record Category(Guid Id, string Name, string Description);
    
public record Clinic(Guid Id, string Name, string Email, string Address,
    string PhoneNumber, string? ProfilePictureUrl);
    
public record Procedure(Guid Id, string Name, string Description,
    int StepIndex, string[] coverImage, ICollection<ProcedurePriceType> procedurePriceTypes
);

public record ProcedurePriceType(Guid Id, string Name, decimal Price);