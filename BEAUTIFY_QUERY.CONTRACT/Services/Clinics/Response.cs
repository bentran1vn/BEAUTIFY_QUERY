namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

public static class Response
{
    public record GetApplyRequest(Guid Id, string Name, string Email, string Address, int TotalApply);
    public record GetApplyRequestById(Guid ApplyId, string Name, string Email, string PhoneNumber,
        string Address, string TaxCode, string BusinessLicenseUrl, string OperatingLicenseUrl,
        DateTimeOffset? OperatingLicenseExpiryDate, string? ProfilePictureUrl, int TotalApply);
}