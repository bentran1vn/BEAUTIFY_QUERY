using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Services;
public class Response
{
    public record GetAllServiceResponse(
        Guid Id,
        string Name,
        decimal MaxPrice,
        decimal MinPrice,
        string discountPercent,
        decimal DiscountMaxPrice,
        decimal DiscountMinPrice,
        ICollection<Image> CoverImage,
        ICollection<Clinic> Clinics,
        Category Category,
        ICollection<DoctorService>? DoctorServices);


    public record DoctorService(
        Guid ServiceId,
        UserEntity Doctor);

    public record UserEntity(
        Guid Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string ProfilePictureUrl,
        ICollection<CertificateEntity> DoctorCertificates
    );

    //Final Test
    public record CertificateEntity
    {
        public Guid Id { get; set; }
        public string CertificateUrl { get; set; }
        public string CertificateName { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? Note { get; set; }
    }

    public record Category(Guid Id, string Name, string Description);


    public record Clinic(
        Guid Id,
        string Name,
        string Email,
        string Address,
        string PhoneNumber,
        string? ProfilePictureUrl,
        bool? IsParent,
        Guid? ParentId);

    public record GetAllServiceByIdResponse(
        Guid Id,
        string Name,
        string Description,
        decimal MaxPrice,
        decimal MinPrice,
        decimal DiscountMaxPrice,
        decimal DiscountMinPrice,
        ICollection<Image> CoverImage,
        ICollection<Image> DescriptionImage,
        ICollection<Clinic> Clinics,
        Category Category,
        ICollection<Procedure> Procedures);

    public record Procedure(
        Guid Id,
        string Name,
        string Description,
        int StepIndex,
        string[] coverImage,
        ICollection<ProcedurePriceType> procedurePriceTypes
    );

    public record ProcedurePriceType(Guid Id, string Name, decimal Price);

    public record Image(Guid Id, int Index, string Url);
}