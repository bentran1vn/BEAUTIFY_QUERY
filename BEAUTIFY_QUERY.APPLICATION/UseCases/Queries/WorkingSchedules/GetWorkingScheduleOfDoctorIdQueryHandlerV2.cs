using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleOfDoctorIdQueryHandlerV2(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWorkingScheduleOfDoctorId,
        PagedResult<Response.GetWorkingScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(
        Query.GetWorkingScheduleOfDoctorId request, CancellationToken cancellationToken)
    {
        var doctorId = currentUserService.UserId;
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        var query = workingScheduleRepository.AsQueryable(x => !x.IsDeleted && x.DoctorId.Equals(doctorId));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = ApplySearchFilter(query, searchTerm);
        }

        query = ApplySorting(query, request.SortOrder);
        var total = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            query,
            request.PageNumber,
            request.PageSize
        );

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
            var part1 = parts[0].Trim();
            var part2 = parts[1].Trim();

            if (DateOnly.TryParse(part1, out var dateFrom) &&
                DateOnly.TryParse(part2, out var dateTo))
            {
                return query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
            }

            if (TimeSpan.TryParse(part1, out var timeFrom) &&
                TimeSpan.TryParse(part2, out var timeTo))
            {
                return query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
            }
        }

        return query.Where(x =>
            (x.CustomerName != null &&
             x.CustomerName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
            x.Status.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase));
    }

    private static IMongoQueryable<WorkingScheduleProjection> ApplySorting(
        IMongoQueryable<WorkingScheduleProjection> query, SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.StartTime)
            : query.OrderBy(x => x.StartTime);
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