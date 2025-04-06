using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.CustomerSchedule)]
public class CustomerScheduleProjection : Document
{
    public string CustomerName;
    public Guid? CustomerId;
    public TimeSpan? StartTime;
    public TimeSpan? EndTime;
    public DateOnly? Date;
    public Guid? ServiceId;
    public string ServiceName;
    public Guid? DoctorId;
    public string? DoctorName;
    public Guid? ClinicId;
    public string ClinicName;
    public string? DoctorNote;
    public required Guid OrderId;
    public string Status;
    public EntityEvent.ProcedurePriceTypeEntity CurrentProcedure;
}