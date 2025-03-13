using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.WorkingSchedule)]
public class WorkingScheduleProjection : Document
{
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public Guid ClinicId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsDeleted { get; set; } = false;
}