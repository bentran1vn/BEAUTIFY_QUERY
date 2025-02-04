using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class Clinics : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(100)] public required string Email { get; set; }
    [MaxLength(15)] public required string PhoneNumber { get; set; }
    [MaxLength(500)] public required string Address { get; set; }
    [MaxLength(20)] public required string TaxCode { get; set; }
    [MaxLength(250)] public required string BusinessLicenseUrl { get; set; }
    [MaxLength(250)] public required string OperatingLicenseUrl { get; set; }
    public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
    public int Status { get; set; } = 0;
    // 0 Pending, 1 Approve, 2 Reject, 3 Banned
    public int TotalApply { get; set; } = 0;
    [MaxLength(250)] public string? ProfilePictureUrl { get; set; }
    public int? TotalBranches { get; set; } = 0;

    public bool IsActivated { get; set; } = false;
    public bool? IsParent { get; set; } = false;
    public Guid? ParentId { get; set; }
    public virtual Clinics? Parent { get; set; }
    [MaxLength(250)] public string? Note { get; set; }
    public virtual ICollection<Clinics> Children { get; set; } = [];
    public virtual ICollection<ClinicOnBoardingRequest>? ClinicOnBoardingRequests { get; set; }
    public virtual ICollection<SystemTransaction>? SystemTransaction { get; set; }


    public virtual ICollection<UserClinic>? UserClinics { get; set; }
    public virtual ICollection<LivestreamRoom>? LivestreamRooms { get; set; }
    public virtual ICollection<Category>? Categories { get; set; }
    public virtual ICollection<ClinicVoucher>? ClinicVouchers { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}