using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;


namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
/// <summary>
/// working-schedules/{clinicId}/working-hours
/// </summary>
internal sealed class GetClinicWorkingHoursQueryHandler(IRepositoryBase<Clinic, Guid> clinicRepositoryBase)
    : IQueryHandler<Query.GetClinicWorkingHours, Response.GetClinicWorkingHoursResponse>
{
    public async Task<Result<Response.GetClinicWorkingHoursResponse>> Handle(Query.GetClinicWorkingHours request,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepositoryBase.FindByIdAsync(request.ClinicId, cancellationToken);
        if (clinic == null)
            return
                Result.Failure<Response.GetClinicWorkingHoursResponse>(new Error("404", "Clinic not found"));
        var response = new Response.GetClinicWorkingHoursResponse
        {
            StartTime = clinic.WorkingTimeStart.Value,
            EndTime = clinic.WorkingTimeEnd.Value,
        };
        return Result.Success(response);
    }
}