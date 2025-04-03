using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class GetCustomerScheduleByIdQueryHandler(
    IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepository) : IQueryHandler<
    Query.GetCustomerScheduleById, Response.CustomerScheduleWithProceduresResponse>
{
    public async Task<Result<Response.CustomerScheduleWithProceduresResponse>> Handle(
        Query.GetCustomerScheduleById request,
        CancellationToken cancellationToken)
    {
        var customerSchedule =
            await customerScheduleRepository.FindByIdAsync(request.Id, cancellationToken, x => x.Customer,
                x => x.Service, x => x.ProcedurePriceType, x => x.ProcedurePriceType!.Procedure);

        if (customerSchedule is null)
            return Result.Failure<Response.CustomerScheduleWithProceduresResponse>(
                new Error("404", "Customer Schedule Not Found !"));

        var currentProcedure = new Response.ProcedureDetailResponse(
            customerSchedule.ProcedurePriceTypeId ?? Guid.Empty,
            customerSchedule.ProcedurePriceType?.Name ?? string.Empty,
            customerSchedule.ProcedurePriceType?.Procedure?.StepIndex.ToString() ?? "0",
            customerSchedule.Date);


        var response = new Response.CustomerScheduleWithProceduresResponse(
            customerSchedule.Id,
            customerSchedule.Customer?.FullName ?? string.Empty,
            customerSchedule.Service?.Name ?? string.Empty,
            customerSchedule.Date,
            customerSchedule.StartTime,
            customerSchedule.EndTime,
            customerSchedule.Status ?? string.Empty,
            currentProcedure);

        return Result.Success(response);
    }
}