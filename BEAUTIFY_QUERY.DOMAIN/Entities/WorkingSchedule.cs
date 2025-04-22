namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class WorkingSchedule : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    public Guid? DoctorId { get; set; }
    public virtual Staff? Doctor { get; set; }


    public Guid? CustomerScheduleId { get; set; }
    public virtual CustomerSchedule? CustomerSchedule { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? ShiftGroupId { get; set; } // Added to group schedules in the same shift

    public virtual ShiftConfig? ShiftGroup { get; set; }
    public int? ShiftCapacity { get; set; } // Added to track how many doctors can work in this shift
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}