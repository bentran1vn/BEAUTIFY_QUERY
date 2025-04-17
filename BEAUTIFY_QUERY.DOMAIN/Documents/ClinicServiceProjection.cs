using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.Service)]
public class ClinicServiceProjection : Document
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<Image> CoverImage { get; set; }
    public Clinic Branding { get; set; }
    public ICollection<EntityEvent.DoctorServiceEntity> DoctorServices { get; set; } = [];
    public decimal DiscountPercent { get; set; } = 0;
    public decimal MaxPrice { get; set; } = 0;
    public decimal MinPrice { get; set; } = 0;
    public decimal DiscountMaxPrice { get; set; } = 0;
    public decimal DiscountMinPrice { get; set; } = 0;
    public Category Category { get; set; }
    public ICollection<Clinic> Clinic { get; set; }
    public ICollection<Procedure> Procedures { get; set; } = Array.Empty<Procedure>();
    public ICollection<Promotion> Promotions { get; set; } = Array.Empty<Promotion>();
    public ICollection<Feedback> Feedbacks { get; set; } = Array.Empty<Feedback>();
}

public record Category(Guid Id, string Name, string Description);

public class Clinic
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public string District { get; set; }
    public string Ward { get; set; }
    public string FullAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool? IsParent { get; set; }
    public bool IsActivated { get; set; }
    public Guid? ParentId { get; set; }
}

public class Procedure
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int StepIndex { get; set; }
    public ICollection<ProcedurePriceType> ProcedurePriceTypes { get; set; } = Array.Empty<ProcedurePriceType>();
}

public class Promotion
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double DiscountPercent { get; set; }
    public string ImageUrl { get; set; }
    public DateTimeOffset StartDay { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActivated { get; set; }
}

public record ProcedurePriceType(Guid Id, string Name, decimal Price, int Duration, bool IsDefault);

public class Image
{
    public Guid Id { get; set; }
    public int Index { get; set; }
    public string Url { get; set; }
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