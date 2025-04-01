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
        var query = repository.AsQueryable(x => !x.IsDeleted);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = ApplySearchFilter(query, searchTerm);
        }

        query = ApplySorting(query, request);

        var workingSchedules = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );

        var result = MapToResponse(workingSchedules);
        return Result.Success(result);
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySearchFilter(
        IMongoQueryable<WorkingScheduleProjection> query, string searchTerm)
    {
        if (!searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            return query.Where(x =>
                x.DoctorName!.Contains(searchTerm) ||
                x.Date.ToString().Contains(searchTerm) ||
                x.StartTime.ToString().Contains(searchTerm) ||
                x.EndTime.ToString().Contains(searchTerm));
        {
            var parts = searchTerm.Split(["to"], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return query.Where(x =>
                    x.DoctorName!.Contains(searchTerm) ||
                    x.Date.ToString().Contains(searchTerm) ||
                    x.StartTime.ToString().Contains(searchTerm) ||
                    x.EndTime.ToString().Contains(searchTerm));
            var part1 = parts[0].Trim();
            var part2 = parts[1].Trim();

            if (TryParseDateRange(part1, part2, out var dateFrom, out var dateTo))
            {
                return query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
            }

            if (TryParseTimeRange(part1, part2, out var timeFrom, out var timeTo))
            {
                return query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
            }
        }

        return query.Where(x =>
            x.DoctorName!.Contains(searchTerm) ||
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
        IMongoQueryable<WorkingScheduleProjection> query, Query.GetWorkingSchedule request)
    {
        return request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
    }

    private static PagedResult<Response.GetWorkingScheduleResponse> MapToResponse(
        PagedResult<WorkingScheduleProjection> workingSchedules)
    {
        var responseItems = workingSchedules.Items.Select(ws => new Response.GetWorkingScheduleResponse
        {
            WorkingScheduleId = ws.DocumentId,
            DoctorId = ws.DoctorId,
            DoctorName = ws.DoctorName,
            Date = ws.Date,
            StartTime = ws.StartTime,
            EndTime = ws.EndTime
        }).ToList();

        return new PagedResult<Response.GetWorkingScheduleResponse>(
            responseItems,
            workingSchedules.PageIndex,
            workingSchedules.PageSize,
            workingSchedules.TotalCount
        );
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