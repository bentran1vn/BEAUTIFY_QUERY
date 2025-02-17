using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleQueryHandler(
    IMongoRepository<WorkingScheduleProjection> repository,IMapper mapper)
    : IQueryHandler<Query.GetWorkingSchedule, PagedResult<Response.GetWorkingScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(Query.GetWorkingSchedule request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        var query = repository.AsQueryable(x => !x.IsDeleted);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x =>
                x.DoctorName.Contains(searchTerm) ||
                x.Date.ToString().Contains(searchTerm) ||
                x.StartTime.ToString().Contains(searchTerm) ||
                x.EndTime.ToString().Contains(searchTerm));
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