using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver.Linq;

/// <summary>
///   api/v{version:apiVersion}/working-schedules/doctor
/// </summary>
namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleOfDoctorIdQueryHandlerV2(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWorkingScheduleOfDoctorId, PagedResult<Response.ShiftGroup>>
{
    public async Task<Result<PagedResult<Response.ShiftGroup>>> Handle(
        Query.GetWorkingScheduleOfDoctorId request, CancellationToken cancellationToken)
    {
        var doctorId = currentUserService.UserId;
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // Query for non-deleted working schedules for the current doctor with customer schedules
        var query = workingScheduleRepository.AsQueryable(x =>
            !x.IsDeleted && x.DoctorId == doctorId && x.CustomerScheduleId != null);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm))
            query = ApplySearchFilter(query, searchTerm);

        // Apply sorting
        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.StartTime)
            : query.OrderBy(x => x.StartTime);

        // Get paged results
        var total = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            query, request.PageNumber, request.PageSize);

        // Map to response
        var result = total.Items
            .GroupBy(x => x.ShiftGroupId)
            .Select(g => new Response.ShiftGroup
            {
                Id = g.First().ShiftGroupId ?? Guid.Empty,
                DoctorId = g.First().DoctorId,
                ClinicId = g.First().ClinicId,
                Date = g.First().Date,
                StartTime = g.Min(x => x.StartTime),
                EndTime = g.Max(x => x.EndTime),
                DoctorName = g.First().DoctorName,
                WorkingSchedules = g.Select(x => new Response.GetWorkingScheduleResponse
                {
                    WorkingScheduleId = x.DocumentId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Status = x.Status,
                    Note = x.Note,
                    StepIndex = x.StepIndex,
                    CustomerName = x.CustomerName,
                    CustomerId = x.CustomerId,
                    ServiceId = x.ServiceId,
                    ServiceName = x.ServiceName,
                    CustomerScheduleId = x.CustomerScheduleId,
                    CurrentProcedureName = x.CurrentProcedureName
                }).ToList()
            })
            .ToList();

        return Result.Success(new PagedResult<Response.ShiftGroup>(
            result, total.PageIndex, total.PageSize, total.TotalCount));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySearchFilter(
        IMongoQueryable<WorkingScheduleProjection> query, string searchTerm)
    {
        // Check if search term contains date or time range
        if (!searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            return query.Where(x =>
                (x.CustomerName != null &&
                 x.CustomerName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
                x.Status.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        {
            var parts = searchTerm.Split("to", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return query.Where(x =>
                    (x.CustomerName != null &&
                     x.CustomerName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
                    x.Status.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase));
            var from = parts[0].Trim();
            var to = parts[1].Trim();

            // Try parsing as date range
            if (DateOnly.TryParse(from, out var dateFrom) && DateOnly.TryParse(to, out var dateTo))
                return query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);

            // Try parsing as time range
            if (TimeSpan.TryParse(from, out var timeFrom) && TimeSpan.TryParse(to, out var timeTo))
                return query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
        }

        // Default search by customer name or status
        return query.Where(x =>
            (x.CustomerName != null &&
             x.CustomerName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
            x.Status.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase));
    }
}