namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class Clinics : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int? TotalBranches { get; set; }
    public string? Status { get; set; }
    public required string TaxCode { get; set; }
    public required string BusinessLicenseUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string OperatingLicenseUrl { get; set; }
    public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
    public bool IsActivated { get; set; } = false;
    public bool? IsParent { get; set; } = false;
    public Guid? ParentId { get; set; }
    public virtual Clinics? Parent { get; set; }
    public string? Note { get; set; }
    public virtual ICollection<Clinics> Children { get; set; }
    public Guid? ClinicOnBoardingRequestId { get; set; }
    public virtual ClinicOnBoardingRequest? ClinicOnBoardingRequest { get; set; }
    public virtual SystemTransaction? SystemTransaction { get; set; }


    public virtual ICollection<UserClinic>? UserClinics { get; set; }
    public virtual ICollection<LivestreamRoom>? LivestreamRooms { get; set; }
    public virtual ICollection<Category>? Categories { get; set; }
    public virtual ICollection<ClinicVoucher>? ClinicVouchers { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}