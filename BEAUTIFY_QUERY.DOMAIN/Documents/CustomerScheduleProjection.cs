using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.CustomerSchedule)]
public class CustomerScheduleProjection : Document
{
    public Guid? ClinicId;
    public string ClinicName;
    public EntityEvent.ProcedurePriceTypeEntity CurrentProcedure;
    public Guid? CustomerId;
    public string CustomerName;
    public DateOnly? Date;
    public Guid? DoctorId;
    public string? DoctorName;
    public string? DoctorNote;
    public TimeSpan? EndTime;
    public required Guid OrderId;
    public Guid? ServiceId;
    public string ServiceName;
    public TimeSpan? StartTime;
    public string Status;
}