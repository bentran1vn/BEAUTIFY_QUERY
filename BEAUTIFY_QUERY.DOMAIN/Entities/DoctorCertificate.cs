using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class DoctorCertificate : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual Staff? Doctor { get; set; }
    [MaxLength(250)] public required string CertificateUrl { get; set; }
    [MaxLength(100)] public required string CertificateName { get; set; }

    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    [MaxLength(100)] public string? Note { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}