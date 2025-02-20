namespace BEAUTIFY_QUERY.CONTRACT.Services.Services;

public class Response
{
    public record GetAllServiceResponse(
        Guid Id, string Name, decimal Price,
        ICollection<string> CoverImage,
        ICollection<Clinic> Clinics, Category Category);
    
    public record Category(Guid Id, string Name, string Description);
    
    public record Clinic(Guid Id, string Name, string Email, string Address,
        string PhoneNumber, string? ProfilePictureUrl, bool? IsParent, Guid? ParentId);
    
    public record GetAllServiceByIdResponse(
        Guid Id, string Name, string Description, decimal Price,
        ICollection<string> CoverImage, ICollection<string> DescriptionImage,
        ICollection<Clinic> Clinics, Category Category, ICollection<Procedure> Procedures);
    
    public record Procedure(Guid Id, string Name, string Description,
        int StepIndex, string[] coverImage, ICollection<ProcedurePriceType> procedurePriceTypes
    );

    public record ProcedurePriceType(Guid Id, string Name, decimal Price);
}