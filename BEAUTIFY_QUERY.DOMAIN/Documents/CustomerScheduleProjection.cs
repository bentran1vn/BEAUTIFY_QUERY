using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_QUERY.DOMAIN.Constrants;

namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.CustomerSchedule)]
public class CustomerScheduleProjection : Document
{
    
    public string StepIndex;
    public string CustomerName;
    public Guid? CustomerId;
    public TimeSpan StartTime;
    public TimeSpan EndTime;
    public DateOnly Date;
    public Guid? ServiceId;
    public string ServiceName;
    public Guid? DoctorId;
    public string? DoctorName;
    public Guid? ClinicId;
    public string ClinicName;
    public string Status;
    public string CurrentProcedureName;
    public ICollection<ProcedurePriceTypeEntity> CompletedProcedures;
    public ICollection<ProcedurePriceTypeEntity> PendingProcedures;
}

public class ProcedurePriceTypeEntity
{
    public Guid Id;
    public string StepIndex;
    public string Name;
    public decimal Price;
    public int Duration;
    public DateOnly DateCompleted;
}