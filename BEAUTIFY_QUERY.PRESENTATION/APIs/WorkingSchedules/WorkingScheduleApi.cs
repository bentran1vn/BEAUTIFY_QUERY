using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Extensions;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.WorkingSchedules;
public class WorkingScheduleApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/working-schedules";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Working Schedules").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet(string.Empty, GetWorkingSchedules).WithSummary("Search theo Date : Date1 to Date2 or Time : Time1 to Time2|| search by DoctorName");
    }

    private static async Task<IResult> GetWorkingSchedules(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetWorkingSchedule(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}