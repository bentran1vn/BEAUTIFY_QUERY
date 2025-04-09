using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class GetAllCustomerScheduleQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepositoryBase)
    : IQueryHandler<Query.GetAllCustomerSchedule, PagedResult<Response.StaffCheckInCustomerScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>> Handle(
        Query.GetAllCustomerSchedule request, CancellationToken cancellationToken)
    {
        var clinicId = currentUserService.ClinicId;
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        var query = customerScheduleRepositoryBase.FindAll(x =>
            !x.IsDeleted && x.Doctor.ClinicId.Equals(clinicId));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // check time
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
                    {
                        query = query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
                    }
                }
            }
            else
            {
                // Fallback to standard contains search with null checks
                // Use EF.Functions.Like for case-insensitive search instead of Contains with StringComparison
                query = query.Where(x =>
                    x.Customer != null &&
                    (x.Customer.FirstName != null &&
                     EF.Functions.Like(x.Customer.FirstName, $"%{searchTerm}%") ||
                     x.Customer.LastName != null &&
                     EF.Functions.Like(x.Customer.LastName, $"%{searchTerm}%")) ||
                    EF.Functions.Like(x.Status, $"%{searchTerm}%"));
            }
        }

        var customerSchedules = await PagedResult<CustomerSchedule>.CreateAsync(
            query.OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ThenBy(x => x.Status),
            request.PageIndex,
            request.PageSize
        );
        var mapped = customerSchedules.Items.Select(x =>
            new Response.StaffCheckInCustomerScheduleResponse(
                x.Id,
                x.OrderId.Value,
                x.Order.FinalAmount.Value,
                x.Customer.FullName,
                x.Customer.PhoneNumber,
                x.Service.Name,
                x.Doctor.User.FullName,
                x.Date,
                x.StartTime,
                x.EndTime, x.Status,
                x.ProcedurePriceType.Name,
                x.ProcedurePriceType?.Procedure.StepIndex.ToString(),
                x.ProcedurePriceType.Procedure.StepIndex == 1
            )).ToList();
        var result = new PagedResult<Response.StaffCheckInCustomerScheduleResponse>(mapped,
            customerSchedules.PageIndex,
            customerSchedules.PageSize,
            customerSchedules.TotalCount);
        return Result.Success(result);
    }
}