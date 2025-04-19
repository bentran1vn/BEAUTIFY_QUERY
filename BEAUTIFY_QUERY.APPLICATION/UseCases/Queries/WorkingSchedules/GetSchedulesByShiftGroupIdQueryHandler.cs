using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
public sealed class GetSchedulesByShiftGroupIdQueryHandler(
    IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetSchedulesByShiftGroupId, PagedResult<Response.GetWorkingScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(
        Query.GetSchedulesByShiftGroupId request, CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;

        // Get base query
        var baseQuery = workingScheduleRepository
            .AsQueryable()
            .Where(x => !x.IsDeleted &&
                        x.ClinicId == currentUserService.ClinicId &&
                        x.ShiftGroupId == request.ShiftGroupId);

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm))
        {
            baseQuery = baseQuery.Where(x =>
                (x.CustomerName != null && x.CustomerName.Contains(searchTerm)) ||
                (x.DoctorName != null && x.DoctorName.Contains(searchTerm)) ||
                (x.ServiceName != null && x.ServiceName.Contains(searchTerm)));
        }

        // Apply sorting
        baseQuery = request.SortOrder == SortOrder.Descending
            ? baseQuery.OrderByDescending(GetSortProperty(request))
            : baseQuery.OrderBy(GetSortProperty(request));

        // Apply pagination
        var pagedItems = await PagedResult<WorkingScheduleProjection>.CreateAsyncMongoLinq(
            baseQuery,
            request.PageNumber,
            request.PageSize
        );

        // Map to response
        var result = pagedItems.Items.Select(MapToResponse).ToList();

        return Result.Success(new PagedResult<Response.GetWorkingScheduleResponse>(
            result,
            pagedItems.PageIndex,
            pagedItems.PageSize,
            pagedItems.TotalCount));
    }


    private static Response.GetWorkingScheduleResponse MapToResponse(WorkingScheduleProjection item)
    {
        return new Response.GetWorkingScheduleResponse
        {
            WorkingScheduleId = item.DocumentId,
            DoctorId = item.DoctorId,
            DoctorName = item.DoctorName,
            Date = item.Date,
            StartTime = item.StartTime,
            EndTime = item.EndTime,
        };
    }

    private static Expression<Func<WorkingScheduleProjection, object>> GetSortProperty(
        Query.GetSchedulesByShiftGroupId request)
    {
        return request.SortColumn switch
        {
            "date" => x => x.Date,
            "startTime" => x => x.StartTime,
            "endTime" => x => x.EndTime,
            _ => x => x.CreatedOnUtc
        };
    }
}