using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetUnregisteredWorkingSchedulesQueryHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository)
    : IQueryHandler<Query.GetUnregisteredWorkingSchedules, PagedResult<Response.GetEmptyScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetEmptyScheduleResponse>>> Handle(
        Query.GetUnregisteredWorkingSchedules request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim().ToLower();
        // Filter working schedules that belong to the current doctor and have no customer schedule
        var query = workingScheduleRepository.AsQueryable(x =>
            x.ClinicId == request.ClinicId &&
            x.DoctorId == null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            {
                var parts = searchTerm.Split("to", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var part1 = parts[0].Trim();
                    var part2 = parts[1].Trim();

                    // Try to parse as a date range
                    if (DateOnly.TryParse(part1, out var dateFrom) &&
                        DateOnly.TryParse(part2, out var dateTo))
                    {
                        query = query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
                    }
                    // Otherwise, try to parse as a time range
                    else if (TimeSpan.TryParse(part1, out var timeFrom) &&
                             TimeSpan.TryParse(part2, out var timeTo))
                    {
                        query = query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
                    }
                    else
                    {
                        // Fallback to standard search if range can't be parsed
                        query = ApplyTextSearch(query, searchTerm);
                    }
                }
                else
                {
                    // Handle malformed "to" expression
                    query = ApplyTextSearch(query, searchTerm);
                }
            }
            else
            {
                // Handle regular search without "to"
                query = ApplyTextSearch(query, searchTerm);
            }
        }

        // Add this helper method

// Before pagination, add:
        query = query.GroupBy(x => new { x.Date, x.StartTime, x.EndTime })
            .Select(g => g.First());
        // Filter by date if provided
        // Apply sorting
        query = ApplySorting(query, request.SortOrder);


        // Get paged result
        var total = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );

        // Map to response
        var result = MapToResponse(total.Items);

        return Result.Success(new PagedResult<Response.GetEmptyScheduleResponse>(
            result,
            total.PageIndex,
            total.PageSize,
            total.TotalCount));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplyTextSearch(
        IMongoQueryable<WorkingScheduleProjection> query, string searchTerm)
    {
        return query.Where(x =>
            (x.DoctorName != null &&
             x.DoctorName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
            (x.Date.ToString().Contains(searchTerm)));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySorting(
        IMongoQueryable<WorkingScheduleProjection> query, SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime)
            : query.OrderBy(x => x.Date).ThenBy(x => x.StartTime);
    }

    private static List<Response.GetEmptyScheduleResponse> MapToResponse(List<WorkingScheduleProjection> items)
    {
        return items.Select(x => new Response.GetEmptyScheduleResponse(x.Date, x.StartTime, x.EndTime)
        ).ToList();
    }
}