using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Booking;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using MongoDB.Driver.Linq;
using User = BEAUTIFY_QUERY.DOMAIN.Entities.User;


namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetBookingPagedResultQueryHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepositoryBase,
    ICurrentUserService currentUserService,
    IRepositoryBase<User, Guid> userRepositoryBase) : IQueryHandler<
    Query.GetBookingPagedResult,
    PagedResult<Response.GetBookingResponse>>
{
    public async Task<Result<PagedResult<Response.GetBookingResponse>>> Handle(Query.GetBookingPagedResult request,
        CancellationToken cancellationToken)
    {
        var user = await userRepositoryBase.FindSingleAsync(
            x => x.Id.Equals(currentUserService.UserId!) && x.Status == 1, cancellationToken);
        if (user is null)
            return Result.Failure<PagedResult<Response.GetBookingResponse>>(new Error("404", "User Not Found !"));
        var searchTerm = request.searchTerm?.Trim().ToLower();
        var query = customerScheduleRepositoryBase.AsQueryable(x => x.CustomerId == user.Id);

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
                        query = query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
                    // Otherwise, try to parse as a time range
                    else if (TimeSpan.TryParse(part1, out var timeFrom) &&
                             TimeSpan.TryParse(part2, out var timeTo))
                        query = query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
                    else
                        // If the range parts can't be parsed, fall back to a standard contains search.
                        query = query.Where(x =>
                            x.Status!.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                            x.ServiceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                        );
                }
                else
                {
                    // If "to" is present but splitting doesn't yield exactly two parts,
                    // use the standard search.
                    query = query.Where(x =>
                        x.Status!.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                        x.ServiceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)
                    );
                }
            }
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        var total = await PagedResult<CustomerScheduleProjection>.CreateAsyncMongoLinq(query, request.PageNumber,
            request.PageSize);
        var mapped = total.Items.Select(x =>
            new Response.GetBookingResponse(
                x.DocumentId,
                x.CustomerName,
                x.StartTime,
                x.EndTime,
                x.ServiceName,
                x.CurrentProcedure.Id,
                x.CurrentProcedure.StepIndex,
                x.CurrentProcedure.Name,
                x.CurrentProcedure.Duration,
                x.CurrentProcedure.DateCompleted,
                x.Status,
                x.Date
            )).ToList();

        return Result.Success(
            new PagedResult<Response.GetBookingResponse>(mapped, total.PageIndex, total.PageSize, total.TotalCount));
    }

    private static Expression<Func<CustomerScheduleProjection, object>> GetSortProperty(
        Query.GetBookingPagedResult request)
    {
        return request.SortColumn switch
        {
            "status" => x => x.Status,
            "service" => x => x.ServiceName,
            "date" => x => x.Date,
            "startTime" => x => x.StartTime,
            "endTime" => x => x.EndTime,
            _ => x => x.CreatedOnUtc
        };
    }
}