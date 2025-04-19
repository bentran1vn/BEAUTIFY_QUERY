using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
public sealed class GetWorkingScheduleByClinicIdQueryHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWorkingScheduleByClinicId, PagedResult<Response.GetScheduleResponseForStaff>>
{
    public async Task<Result<PagedResult<Response.GetScheduleResponseForStaff>>> Handle(
        Query.GetWorkingScheduleByClinicId request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // Filter by clinic ID and not deleted
        var baseQuery =
            workingScheduleRepository.AsQueryable(x => !x.IsDeleted && x.ClinicId == currentUserService.ClinicId);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm)) baseQuery = ApplySearchFilter(baseQuery, searchTerm);

        // Calculate capacities based on all matching records using efficient aggregation
        var capacities = await CalculateCapacities(baseQuery);

        // Apply sorting
        var sortedQuery = ApplySorting(baseQuery, request.SortOrder);

        // Create a distinct query (grouped by date/time) for pagination
        var distinctQuery = sortedQuery
            .GroupBy(x => new { x.Date, x.StartTime, x.EndTime })
            .Select(g => new WorkingScheduleProjection
            {
                Date = g.Key.Date,
                StartTime = g.Key.StartTime,
                EndTime = g.Key.EndTime
            });

        // Apply pagination to the distinct query
        var pagedItems = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            distinctQuery,
            request.PageNumber,
            request.PageSize
        );

        // Map to response using pre-calculated capacities
        var result = MapToResponseWithCapacities(pagedItems.Items, capacities);

        return Result.Success(new PagedResult<Response.GetScheduleResponseForStaff>(
            result,
            pagedItems.PageIndex,
            pagedItems.PageSize,
            pagedItems.TotalCount));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySearchFilter(
        IMongoQueryable<WorkingScheduleProjection> query, string searchTerm)
    {
        if (!searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            return query.Where(x =>
                (x.DoctorName != null && x.DoctorName.Contains(searchTerm)) ||
                (x.CustomerName != null && x.CustomerName.Contains(searchTerm)) ||
                x.Date.ToString().Contains(searchTerm) ||
                x.StartTime.ToString().Contains(searchTerm) ||
                x.EndTime.ToString().Contains(searchTerm));
        {
            var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return query.Where(x =>
                    (x.DoctorName != null && x.DoctorName.Contains(searchTerm)) ||
                    (x.CustomerName != null && x.CustomerName.Contains(searchTerm)) ||
                    x.Date.ToString().Contains(searchTerm) ||
                    x.StartTime.ToString().Contains(searchTerm) ||
                    x.EndTime.ToString().Contains(searchTerm));
            var part1 = parts[0].Trim();
            var part2 = parts[1].Trim();

            if (TryParseDateRange(part1, part2, out var dateFrom, out var dateTo))
                return query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);

            if (TryParseTimeRange(part1, part2, out var timeFrom, out var timeTo))
                return query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
        }

        return query.Where(x =>
            (x.DoctorName != null && x.DoctorName.Contains(searchTerm)) ||
            (x.CustomerName != null && x.CustomerName.Contains(searchTerm)) ||
            x.Date.ToString().Contains(searchTerm) ||
            x.StartTime.ToString().Contains(searchTerm) ||
            x.EndTime.ToString().Contains(searchTerm));
    }

    private static bool TryParseDateRange(string part1, string part2, out DateOnly dateFrom, out DateOnly dateTo)
    {
        dateFrom = default;
        dateTo = default;
        return DateOnly.TryParse(part1, out dateFrom) && DateOnly.TryParse(part2, out dateTo);
    }

    private static bool TryParseTimeRange(string part1, string part2, out TimeSpan timeFrom, out TimeSpan timeTo)
    {
        timeFrom = TimeSpan.Zero;
        timeTo = TimeSpan.Zero;
        return TimeSpan.TryParse(part1, out timeFrom) && TimeSpan.TryParse(part2, out timeTo);
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySorting(
        IMongoQueryable<WorkingScheduleProjection> query, SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime)
            : query.OrderBy(x => x.Date).ThenBy(x => x.StartTime);
    }

    private async Task<Dictionary<(DateOnly Date, TimeSpan StartTime, TimeSpan EndTime), int>> CalculateCapacities(
        IMongoQueryable<WorkingScheduleProjection> query)
    {
        // Use MongoDB aggregation to calculate capacities on the database side
        var capacities = await query
            .GroupBy(x => new { x.Date, x.StartTime, x.EndTime })
            .Select(g => new
            {
                Key = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Convert to dictionary for faster lookups
        return capacities.ToDictionary(
            item => (item.Key.Date, item.Key.StartTime, item.Key.EndTime),
            item => item.Count
        );
    }

    private static List<Response.GetScheduleResponseForStaff> MapToResponseWithCapacities(
        List<WorkingScheduleProjection> items,
        Dictionary<(DateOnly Date, TimeSpan StartTime, TimeSpan EndTime), int> capacities)
    {
        return items
            .Select(item => new Response.GetScheduleResponseForStaff(
                // Look up the capacity for this time slot from the pre-calculated dictionary
                capacities.TryGetValue((item.Date, item.StartTime, item.EndTime), out var capacity) ? capacity : 0,
                item.Date,
                item.StartTime,
                item.EndTime
            ))
            .ToList();
    }
}