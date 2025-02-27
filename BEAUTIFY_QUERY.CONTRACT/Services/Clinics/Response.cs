namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
public static class Response
{
    public record GetClinics(Guid Id, string Name, string Email, string Address, int TotalBranches, bool IsActivated);

    public record GetClinicBranches(
        Guid Id,
        string Name,
        string Email,
        string Address,
        string TaxCode,
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
        string Address,
        string TaxCode,
        string BusinessLicenseUrl,
        string OperatingLicenseUrl,
        DateTimeOffset? OperatingLicenseExpiryDate,
        string? ProfilePictureUrl,
        int TotalBranches,
        bool IsActivated);

    public record GetApplyRequest(Guid Id, string Name, string Email, string Address, int TotalApply);

    public record GetApplyRequestById(
        Guid ApplyId,
        string Name,
        string Email,
        string PhoneNumber,
        string Address,
        string TaxCode,
        string BusinessLicenseUrl,
        string OperatingLicenseUrl,
        DateTimeOffset? OperatingLicenseExpiryDate,
        string? ProfilePictureUrl,
        int TotalApply);

    public class GetAccountOfEmployee
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Role { get; set; }
    }
}