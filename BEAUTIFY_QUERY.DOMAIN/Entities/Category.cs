

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Category : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActivated { get; set; } = false;
    public bool IsParent { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public Guid? ParentId { get; set; }
    public virtual Category? Parent { get; set; }

    public virtual ICollection<Category> Children { get; set; }
    public Guid? ClinicId { get; set; }
    public virtual Clinics? Clinic { get; set; }
    public virtual ICollection<Service> Services { get; set; }
    public virtual ICollection<DoctorService>? DoctorServices { get; set; }
}