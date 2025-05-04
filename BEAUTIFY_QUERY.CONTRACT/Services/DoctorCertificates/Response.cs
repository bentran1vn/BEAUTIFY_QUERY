namespace BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
public static class Response
{
    public class GetDoctorCertificateByResponse
    {
        public Guid? Id { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string DoctorName { get; set; }
        public string CertificateUrl { get; set; }
        public string CertificateName { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? Note { get; set; }
    }
}