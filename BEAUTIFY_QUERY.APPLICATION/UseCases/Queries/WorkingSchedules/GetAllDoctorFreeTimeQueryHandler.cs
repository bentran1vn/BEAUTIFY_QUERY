using System.Collections.Immutable;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetAllDoctorFreeTimeQueryHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : IQueryHandler<Query.GetAllDoctorFreeTime, IReadOnlyList<Response.DoctorBusyTimeInADay>>
{
    public async Task<Result<IReadOnlyList<Response.DoctorBusyTimeInADay>>> Handle(Query.GetAllDoctorFreeTime request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkingScheduleProjection> result = repository.FilterBy(x =>
                x.DoctorId == request.DoctorId && x.ClinicId == request.ClinicId && x.Date == request.Date)
            .ToImmutableList();

        IReadOnlyList<Response.DoctorBusyTimeInADay> doctorBusyTimeInADay = result.Select(x =>
            new Response.DoctorBusyTimeInADay
            {
                Start = x.StartTime,
                End = x.EndTime,
                Date = x.Date
            }).ToList();

        return Result.Success(doctorBusyTimeInADay);
    }
}