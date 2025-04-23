using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class CustomerSchedule : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public virtual User? Customer { get; set; }
    public Guid ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public Guid DoctorId { get; set; }
    public virtual UserClinic? Doctor { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public DateOnly? Date { get; set; }
    public Guid? FeedbackId { get; set; }
    public virtual Feedback? Feedback { get; set; }
    [MaxLength(50)] public string? Status { get; set; }
    [MaxLength(2000)] public string? DoctorNote { get; set; }
    public Guid? ProcedurePriceTypeId { get; set; }
    public virtual ProcedurePriceTypes? ProcedurePriceType { get; set; }
    
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}