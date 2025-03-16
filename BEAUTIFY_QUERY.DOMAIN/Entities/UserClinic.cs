﻿namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class UserClinic : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    public virtual Staff? User { get; set; }
    public virtual ICollection<WorkingSchedule>? WorkingSchedules { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}