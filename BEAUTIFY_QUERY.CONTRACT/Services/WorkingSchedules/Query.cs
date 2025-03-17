using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
public static class Query
{
    public record GetWorkingSchedule(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.GetWorkingScheduleResponse>>;

    public record GetAllDoctorFreeTime(Guid DoctorId,Guid ClinicId,DateOnly Date) : IQuery<IReadOnlyList<Response.DoctorBusyTimeInADay>>;
}