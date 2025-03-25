using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleQueryHandler(
    IMongoRepository<WorkingScheduleProjection> repository)
    : IQueryHandler<Query.GetWorkingSchedule, PagedResult<Response.GetWorkingScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(Query.GetWorkingSchedule request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim();
        var query = repository.AsQueryable(x => !x.IsDeleted );
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (Guid.TryParse(searchTerm, out var doctorGuid))
            {
                query = query.Where(x => x.DoctorId == doctorGuid);
            }
            // If the search term appears to be a range (e.g. "2025-02-01 to 2025-02-15" or "08:00 to 17:00")
            if (searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            {
                var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var part1 = parts[0].Trim();
                    var part2 = parts[1].Trim();

                    // Try to parse as a date range
                    if (DateOnly.TryParse(part1, out var dateFrom) &&
                        DateOnly.TryParse(part2, out var dateTo))
                        query = query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
                    // Otherwise, try to parse as a time range
                    else if (TimeSpan.TryParse(part1, out var timeFrom) &&
                             TimeSpan.TryParse(part2, out var timeTo))
                        query = query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
                    else
                        // If the range parts can't be parsed, fall back to a standard contains search.
                        query = query.Where(x =>
                            x.DocumentId.ToString().Contains(searchTerm) ||
                            x.DoctorId.ToString().Contains(searchTerm) ||
                            x.DoctorName!.Contains(searchTerm) ||
                            x.Date.ToString().Contains(searchTerm) ||
                            x.StartTime.ToString().Contains(searchTerm) ||
                            x.EndTime.ToString().Contains(searchTerm));
                }
                else
                {
                    // If "to" is present but splitting doesn't yield exactly two parts,
                    // use the standard search.
                    query = query.Where(x =>
                        x.DocumentId.ToString().Contains(searchTerm) ||
                        x.DoctorId.ToString().Contains(searchTerm) ||
                        x.DoctorName!.Contains(searchTerm) ||
                        x.Date.ToString().Contains(searchTerm) ||
                        x.StartTime.ToString().Contains(searchTerm) ||
                        x.EndTime.ToString().Contains(searchTerm));
                }
            }
            else
            {
                query = query.Where(x =>
                    //x.DocumentId.ToString().Contains(searchTerm) ||
                    x.DoctorId!.Value.ToString().Contains(searchTerm) ||
                    x.DoctorName!.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                );
            }
        }


        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
        var workingSchedules = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );
        var r1 = workingSchedules.Items.Select(ws => new Response.GetWorkingScheduleResponse
        {
            WorkingScheduleId = ws.DocumentId,
            DoctorId = ws.DoctorId,
            DoctorName = ws.DoctorName,
            Date = ws.Date,
            Start = ws.StartTime,
            End = ws.EndTime
        }).ToList();
        var result = new PagedResult<Response.GetWorkingScheduleResponse>(
            r1,
            workingSchedules.PageIndex,
            workingSchedules.PageSize,
            workingSchedules.TotalCount
        );
        return Result.Success(result);
    }

    private static Expression<Func<WorkingScheduleProjection, object>> GetSortProperty(Query.GetWorkingSchedule request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "date" => projection => projection.Date,
            _ => projection => projection.CreatedOnUtc
        };
    }
}