namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
public static class Response
{
    public class GetClinicBranchesResponse
    {
        public List<ClinicBranchDto> Clinics { get; set; } = new();
        public TotalSummaryDto Totals { get; set; } = new();
    }

    public class ClinicBranchDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public decimal Balance { get; set; }
        public TimeSpan? WorkingTimeStart { get; set; }
        public TimeSpan? WorkingTimeEnd { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
        public string FullAddress => $"{Address}, {Ward}, {District}, {City}".Trim(',', ' ', '\n');
        public string TaxCode { get; set; }
        public string BusinessLicenseUrl { get; set; }
        public string OperatingLicenseUrl { get; set; }
        public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
        public decimal PendingWithdrawals { get; set; }

        public decimal TotalEarnings { get; set; }

        //bank account
        public string? BankName { get; set; } = string.Empty;
        public string? BankAccountNumber { get; set; } = string.Empty;
        public bool? IsMainClinic { get; set; } = false;
    }

    public class TotalSummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalPendingWithdrawals { get; set; }
        public decimal TotalEarnings { get; set; }
    }

    public record GetClinics(
        Guid Id,
        string Name,
        string Email,
        string FullAddress,
        int TotalBranches,
        string ProfilePictureUrl,
        bool IsActivated);

    public record GetClinicBranches(
        Guid Id,
        string Name,
        string Email,
        string? City,
        string? Address,
        string? District,
        string? Ward,
        string? FullAddress,
        string TaxCode,
        TimeSpan? WorkingTimeStart,
        TimeSpan? WorkingTimeEnd,
        string BusinessLicenseUrl,
        string OperatingLicenseUrl,
        DateTimeOffset? OperatingLicenseExpiryDate,
        string? ProfilePictureUrl,
        bool IsActivated);

    public record GetClinicDetail(
        Guid Id,
        string Name,
        string Email,
        string PhoneNumber,
        string? City,
        string? Address,
        string? District,
        string? Ward,
        string? FullAddress,
        int BranchLimit,
        int LivestreamLimit,
        string TaxCode,
        string BusinessLicenseUrl,
        string OperatingLicenseUrl,
        TimeSpan? WorkingTimeStart,
        TimeSpan? WorkingTimeEnd,
        DateTimeOffset? OperatingLicenseExpiryDate,
        string? ProfilePictureUrl,
        int TotalBranches,
        bool IsActivated,
        int Status,
        string BankName,
        string BankAccountNumber,
        Subscription? currentSubscription = null,
        PagedResult<GetClinicDetail>? Branches = null,
        List<Services.Response.GetAllServiceInGetClinicById>? Services = null);

    public class MyClinicApply
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BusinessLicense { get; set; }
        public string OperatingLicense { get; set; }
        public DateTimeOffset OperatingLicenseExpiryDate { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string RejectReason { get; set; }
    }

    public record Subscription(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int Duration,
        bool IsActivated,
        int LimitBranch,
        int LimitLiveStream,
        DateOnly DateBought,
        DateOnly DateExpired,
        int DaysLeft);

    public record GetApplyRequest(
        Guid Id,
        string Name,
        string Email,
        string? FullAddress,
        int TotalApply,
        DateTimeOffset DateApplied);

    public class BranchClinicApplyGetAll
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }

        public Guid ParentId { get; set; }
        public string ParentName { get; set; }
        public string ParentEmail { get; set; }
        public string ParentPhoneNumber { get; set; }
        public string ParentCity { get; set; }
        public string ParentDistrict { get; set; }
        public string ParentWard { get; set; }
        public string ParentAddress { get; set; }

        public string? RejectReason { get; set; }
    }

    public class BranchClinicApplyDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string FullAddress { get; set; }
        public TimeSpan? WorkingTimeStart { get; set; }
        public TimeSpan? WorkingTimeEnd { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BusinessLicenseUrl { get; set; }
        public string OperatingLicenseUrl { get; set; }
        public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
        public string ProfilePictureUrl { get; set; }

        public Guid ParentId { get; set; }
        public string ParentName { get; set; }
        public string ParentEmail { get; set; }
        public string ParentPhoneNumber { get; set; }
        public string ParentCity { get; set; }
        public string ParentDistrict { get; set; }
        public string ParentWard { get; set; }
        public string ParentAddress { get; set; }

        public string? RejectReason { get; set; }
    }

    public record GetApplyRequestById(
        Guid ApplyId,
        string Name,
        string Email,
        string PhoneNumber,
        string? City,
        string? Address,
        string? District,
        string? Ward,
        string? FullAddress,
        string TaxCode,
        string BusinessLicenseUrl,
        string OperatingLicenseUrl,
        DateTimeOffset? OperatingLicenseExpiryDate,
        string? ProfilePictureUrl,
        int TotalApply);

    public class GetAccountOfEmployee
    {
        public GetClinicBranches[] Branchs { get; set; }
        public Guid EmployeeId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullAddress { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Role { get; set; }
        public List<DoctorCertificates>? DoctorCertificates { get; set; }
    }

    public class DoctorCertificates
    {
        public Guid Id { get; set; }
        public string CertificateUrl { get; set; }
        public string CertificateName { get; set; }

        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? Note { get; set; }
    }
}