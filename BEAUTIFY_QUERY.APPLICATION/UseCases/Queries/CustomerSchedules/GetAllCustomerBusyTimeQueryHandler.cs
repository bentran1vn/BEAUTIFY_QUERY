using System.Collections.Immutable;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class GetAllCustomerBusyTimeQueryHandler(IMongoRepository<CustomerScheduleProjection> repository)
    : IQueryHandler<Query.GetAllCustomerBusyTime, IReadOnlyList<Response.CustomerBusyTimeInADay>>
{
    public async Task<Result<IReadOnlyList<Response.CustomerBusyTimeInADay>>> Handle(
        Query.GetAllCustomerBusyTime request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CustomerScheduleProjection> result = repository.FilterBy(x =>
                x.CustomerId == request.CustomerId && x.Date == request.Date)
            .ToImmutableList();

        IReadOnlyList<Response.CustomerBusyTimeInADay> customerBusyTimeInADay = result.Select(x =>
            new Response.CustomerBusyTimeInADay(
                x.StartTime,
                x.EndTime,
                x.Date
            )).ToList();

        return Result.Success(customerBusyTimeInADay);
    }
}