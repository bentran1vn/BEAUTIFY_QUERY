using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class GetAllCustomerScheduleQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepositoryBase)
    : IQueryHandler<Query.GetAllCustomerSchedule, PagedResult<Response.StaffCheckInCustomerScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>> Handle(
        Query.GetAllCustomerSchedule request, CancellationToken cancellationToken)
    {
        if (currentUserService.Role != Constant.Role.CLINIC_STAFF &&
            currentUserService.Role != Constant.Role.CLINIC_ADMIN)
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(new Error("403",
                "You do not have permission to access this resource."));
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
                query = query.Where(x =>
                    x.Customer != null &&
                    (x.Customer.FirstName != null && x.Customer.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                     x.Customer.LastName != null && x.Customer.LastName.ToLower().Contains(searchTerm.ToLower())));
            }
        }

        var customerSchedules = await PagedResult<CustomerSchedule>.CreateAsync(
            query,
            request.PageIndex,
            request.PageSize
        );
        var mapped = customerSchedules.Items.Select(x =>
            new Response.StaffCheckInCustomerScheduleResponse(
                x.Id,
                x.OrderId.Value,
                x.Customer.FullName,
                x.Customer.PhoneNumber,
                x.Service.Name,
                x.Doctor.User.FullName,
                x.Date,
                x.StartTime,
                x.EndTime, x.Status,
                x.ProcedurePriceType.Name
            )).ToList();
        var result = new PagedResult<Response.StaffCheckInCustomerScheduleResponse>(mapped,
            customerSchedules.PageIndex,
            customerSchedules.PageSize,
            customerSchedules.TotalCount);
        return Result.Success(result);
    }
}