﻿namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class WorkingSchedule : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? DoctorClinicId { get; set; }
    public virtual UserClinic? DoctorClinic { get; set; }
    public Guid? CustomerScheduleId { get; set; }
    public virtual CustomerSchedule? CustomerSchedule { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}