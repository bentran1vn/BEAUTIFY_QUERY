using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class GetCustomerScheduleByIdQueryHandler(
    IMongoRepository<CustomerScheduleProjection> customerScheduleRepository) : IQueryHandler<
    Query.GetCustomerScheduleById, Response.CustomerScheduleWithProceduresResponse>
{
    public async Task<Result<Response.CustomerScheduleWithProceduresResponse>> Handle(
        Query.GetCustomerScheduleById request,
        CancellationToken cancellationToken)
    {
        var customerSchedule = await customerScheduleRepository.FindOneAsync(x => x.DocumentId == request.Id);
        if (customerSchedule is null)
            return Result.Failure<Response.CustomerScheduleWithProceduresResponse>(
                new Error("404", "Customer Schedule Not Found !"));
        var currentProcedure = new Response.ProcedureDetailResponse(
            customerSchedule.CurrentProcedure.Id,
            customerSchedule.CurrentProcedure.Name,
            customerSchedule.CurrentProcedure.StepIndex,
            customerSchedule.Date);
        var response = new Response.CustomerScheduleWithProceduresResponse(
            customerSchedule.DocumentId,
            customerSchedule.CustomerName,
            customerSchedule.ServiceName,
            customerSchedule.Date,
            customerSchedule.StartTime,
            customerSchedule.EndTime,
            customerSchedule.Status,
            customerSchedule.DoctorNote ?? string.Empty,
            customerSchedule.DoctorId.Value,
            currentProcedure);
        return Result.Success(response);
    }
}