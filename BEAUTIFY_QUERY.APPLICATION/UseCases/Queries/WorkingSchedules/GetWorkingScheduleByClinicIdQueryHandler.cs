using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleByClinicIdQueryHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWorkingScheduleByClinicId, PagedResult<Response.GetWorkingScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(
        Query.GetWorkingScheduleByClinicId request, CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // Filter by clinic ID and not deleted
        var query =
            workingScheduleRepository.AsQueryable(x => !x.IsDeleted && x.ClinicId == currentUserService.ClinicId);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm)) query = ApplySearchFilter(query, searchTerm);

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

        return Result.Success(new PagedResult<Response.GetWorkingScheduleResponse>(
            result,
            total.PageIndex,
            total.PageSize,
            total.TotalCount));
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
        timeFrom = default;
        timeTo = default;
        return TimeSpan.TryParse(part1, out timeFrom) && TimeSpan.TryParse(part2, out timeTo);
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySorting(
        IMongoQueryable<WorkingScheduleProjection> query, SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime)
            : query.OrderBy(x => x.Date).ThenBy(x => x.StartTime);
    }

    private static List<Response.GetWorkingScheduleResponse> MapToResponse(List<WorkingScheduleProjection> items)
    {
        return items.Select(x => new Response.GetWorkingScheduleResponse
        {
            WorkingScheduleId = x.DocumentId,
            DoctorName = x.DoctorName,
            ClinicId = x.ClinicId,
            DoctorId = x.DoctorId,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Date = x.Date,
            Status = x.Status,
            StepIndex = x.StepIndex,
            CustomerName = x.CustomerName,
            CustomerId = x.CustomerId,
            ServiceId = x.ServiceId,
            ServiceName = x.ServiceName,
            CustomerScheduleId = x.CustomerScheduleId,
            CurrentProcedureName = x.CurrentProcedureName
        }).ToList();
    }
}