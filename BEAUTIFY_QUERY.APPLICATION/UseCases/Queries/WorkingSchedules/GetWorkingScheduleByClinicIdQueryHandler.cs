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
    private const string ToSeparator = "to";

    public async Task<Result<PagedResult<Response.GetScheduleResponseForStaff>>> Handle(
        Query.GetWorkingScheduleByClinicId request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // Get base query
        var baseQuery = workingScheduleRepository
            .AsQueryable()
            .Where(x => !x.IsDeleted && x.ClinicId == currentUserService.ClinicId);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm))
            baseQuery = ApplySearchFilter(baseQuery, searchTerm);

        // Calculate capacities
        var capacityLookup = await CalculateCapacities(baseQuery, cancellationToken);

        // Apply sorting
        var sortedQuery = ApplySorting(baseQuery, request.SortOrder);

        // Create distinct query for pagination
        var distinctQuery = sortedQuery
            .GroupBy(x => new { x.Date, x.StartTime, x.EndTime, x.ShiftGroupId })
            .Select(g => new WorkingScheduleProjection
            {
                Date = g.Key.Date,
                StartTime = g.Key.StartTime,
                EndTime = g.Key.EndTime,
                ShiftGroupId = g.Key.ShiftGroupId,
            });

        // Apply pagination
        var pagedItems = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            distinctQuery,
            request.PageNumber,
            request.PageSize
        );

        // Map to response
        var result = MapToResponseWithCapacities(pagedItems.Items, capacityLookup);

        return Result.Success(new PagedResult<Response.GetScheduleResponseForStaff>(
            result,
            pagedItems.PageIndex,
            pagedItems.PageSize,
            pagedItems.TotalCount));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySearchFilter(
        IMongoQueryable<WorkingScheduleProjection> query, string searchTerm)
    {
        try
        {
            // Check if it's a range search
            var toIndex = searchTerm.IndexOf(ToSeparator, StringComparison.OrdinalIgnoreCase);

            if (toIndex < 0 || searchTerm.Length <= toIndex + 2)
            {
                // Try to parse as date or time
                if (DateOnly.TryParse(searchTerm, out var searchDate))
                    return query.Where(x => x.Date == searchDate);

                if (!TimeSpan.TryParse(searchTerm, out var searchTime)) return query;
                {
                    var timeMinutes = (int)searchTime.TotalMinutes;
                    return query.Where(x =>
                        x.StartTime.TotalMinutes <= timeMinutes &&
                        x.EndTime.TotalMinutes >= timeMinutes);
                }
            }

            // Handle range search
            var part1 = searchTerm[..toIndex].Trim();
            var part2 = searchTerm[(toIndex + 2)..].Trim();

            // Try to parse as date range
            if (DateOnly.TryParse(part1, out var dateFrom) && DateOnly.TryParse(part2, out var dateTo))
                return query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);

            // Try to parse as time range
            if (TimeSpan.TryParse(part1, out var startTime) && TimeSpan.TryParse(part2, out var endTime))
                return query.Where(x => x.StartTime >= startTime && x.EndTime <= endTime);

            return query;
        }
        catch
        {
            return query;
        }
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySorting(
        IMongoQueryable<WorkingScheduleProjection> query, SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime)
            : query.OrderBy(x => x.Date).ThenBy(x => x.StartTime);
    }

    private async Task<Dictionary<(DateOnly, TimeSpan, TimeSpan), (int Capacity, int DoctorCount, int CustomerCount)>>
        CalculateCapacities(
            IMongoQueryable<WorkingScheduleProjection> query,
            CancellationToken cancellationToken)
    {
        var capacityResults = await query
            .GroupBy(x => new { x.Date, x.StartTime, x.EndTime })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.StartTime,
                g.Key.EndTime,
                TotalCount = g.Count(),
                DoctorCount = g.Count(x => x.DoctorId != null),
                CustomerCount = g.Count(x => x.CustomerScheduleId != null)
            })
            .ToListAsync(cancellationToken);

        var capacityDict =
            new Dictionary<(DateOnly, TimeSpan, TimeSpan), (int Capacity, int DoctorCount, int CustomerCount)>();
        foreach (var item in capacityResults)
        {
            capacityDict[(item.Date, item.StartTime, item.EndTime)] =
                (item.TotalCount, item.DoctorCount, item.CustomerCount);
        }

        return capacityDict;
    }

    private static List<Response.GetScheduleResponseForStaff> MapToResponseWithCapacities(
        List<WorkingScheduleProjection> items,
        Dictionary<(DateOnly, TimeSpan, TimeSpan), (int Capacity, int DoctorCount, int CustomerCount)> capacities)
    {
        var result = new List<Response.GetScheduleResponseForStaff>();

        foreach (var item in items)
        {
            var key = (item.Date, item.StartTime, item.EndTime);
            var (capacity, doctorCount, customerCount) =
                capacities.TryGetValue(key, out var counts) ? counts : (0, 0, 0);

            result.Add(new Response.GetScheduleResponseForStaff(
                item.ShiftGroupId.Value,
                capacity,
                doctorCount,
                customerCount,
                item.Date,
                item.StartTime,
                item.EndTime
            ));
        }

        return result;
    }
}