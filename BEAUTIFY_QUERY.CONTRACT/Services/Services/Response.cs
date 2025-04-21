namespace BEAUTIFY_QUERY.CONTRACT.Services.Services;
public class Response
{
    public record GetAllServiceResponse(
        Guid Id,
        string Name,
        Clinic Branding,
        decimal MaxPrice,
        decimal MinPrice,
        double DepositPercent,
        bool IsRefundable,
        string DiscountPercent,
        decimal DiscountMaxPrice,
        decimal DiscountMinPrice,
        ICollection<Image> CoverImage,
        ICollection<Clinic> Clinics,
        Category Category,
        ICollection<DoctorService>? DoctorServices,
        ICollection<Feedback> Feedbacks);


    public record DoctorService(
        Guid Id,
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
        TimeSpan WorkingTimeStart,
        TimeSpan WorkingTimeEnd,
        string? ProfilePictureUrl,
        bool? IsParent,
        bool IsActivated,
        Guid? ParentId);

    public class GetAllServiceInGetClinicById
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public required double DepositPercent { get; set; }
        public required bool IsRefundable { get; set; }
        public string DiscountPercent { get; set; }
        public decimal DiscountMaxPrice { get; set; }
        public decimal DiscountMinPrice { get; set; }
        public ICollection<Image> CoverImage { get; set; }
    }

    public class Feedback
    {
        public Guid FeedbackId { get; set; }
        public Guid ServiceId { get; set; }
        public ICollection<string> Images { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public User User { get; set; }
        public bool IsView { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }


    public record GetAllServiceByIdResponse(
        Guid Id,
        string Name,
        string Description,
        Clinic Branding,
        decimal MaxPrice,
        decimal MinPrice,
        string DiscountPercent,
        decimal DiscountMaxPrice,
        decimal DiscountMinPrice,
        double DepositPercent,
        bool IsRefundable,
        ICollection<Image> CoverImage,
        ICollection<Clinic>? Clinics,
        Category Category,
        ICollection<Procedure> Procedures,
        ICollection<Promotion>? Promotions,
        ICollection<DoctorService>? DoctorServices,
        ICollection<Feedback> Feedbacks);
    
    public record GetAllDoctorServiceByIdResponse(
        Guid Id,
        ICollection<DoctorService>? DoctorServices);

    public record Procedure(
        Guid Id,
        string Name,
        string Description,
        int StepIndex,
        ICollection<ProcedurePriceType> procedurePriceTypes
    );

    public record ProcedurePriceType(Guid Id, string Name, int Duration, decimal Price, bool IsDefault);

    public record Image(Guid Id, int Index, string Url);

    public record Promotion(
        Guid Id,
        string Name,
        double DiscountPercent,
        string ImageUrl,
        DateTimeOffset StartDay,
        DateTimeOffset EndDate,
        bool IsActivated);
}