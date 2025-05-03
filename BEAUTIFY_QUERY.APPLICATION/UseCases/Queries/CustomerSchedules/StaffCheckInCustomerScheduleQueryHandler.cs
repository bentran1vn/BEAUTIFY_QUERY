using System.Linq.Expressions;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using Microsoft.EntityFrameworkCore;
using User = BEAUTIFY_QUERY.DOMAIN.Entities.User;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.CustomerSchedules;
internal sealed class StaffCheckInCustomerScheduleQueryHandler(
    IRepositoryBase<User, Guid> userRepositoryBase,
    ICurrentUserService currentUserService,
    IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepositoryBase)
    : IQueryHandler<Query.StaffCheckInCustomerScheduleQuery, PagedResult<Response.StaffCheckInCustomerScheduleResponse>>
{
    public async Task<Result<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>> Handle(
        Query.StaffCheckInCustomerScheduleQuery request, CancellationToken cancellationToken)
    {
        // If search term is provided, search by schedule ID and ignore other filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Try to parse the search term as a GUID to check if it's a valid schedule ID
            if (Guid.TryParse(request.SearchTerm, out Guid scheduleId))
            {
                // Search directly for the schedule with this ID
                var schedule = await customerScheduleRepositoryBase.FindAll(x => x.Id == scheduleId &&
                        x.Doctor.ClinicId == currentUserService.ClinicId &&
                        x.StartTime != null)
                    .Include(x => x.Service)
                    .Include(x => x.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(x => x.ProcedurePriceType)
                    .Include(x => x.Order)
                    .OrderBy(x => x.StartTime)
                    .FirstOrDefaultAsync(cancellationToken);

                if (schedule == null)
                    return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                        new Error("404", "No customer schedule found with the provided ID"));

                // Get the user information
                var user = await userRepositoryBase.FindByIdAsync(schedule.CustomerId);

                if (user == null)
                    return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                        new Error("404", "Customer information not found"));

                // Create a single-item paged result - using different variable names to avoid conflicts
                var singleUserDict = new Dictionary<Guid, User> { { user.Id, user } };
                var singleResponse = MapToResponse(schedule, singleUserDict);

                var singleResult = new PagedResult<Response.StaffCheckInCustomerScheduleResponse>(
                    new List<Response.StaffCheckInCustomerScheduleResponse> { singleResponse },
                    1,
                    1,
                    1
                );

                return Result.Success(singleResult);
            }
            else
            {
                return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                    new Error("400", "Invalid schedule ID format"));
            }
        }

        // Original logic for filtering by customer name and phone if search term is not provided
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("400", "Customer name is required"));

        // The rest of your original code remains unchanged
        var userFilter = CreateUserFilter(request);

        var users = await userRepositoryBase.FindAll(userFilter)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("404", "No matching users found"));

        var VietNameTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietNameTimeZone);

        var query = customerScheduleRepositoryBase.FindAll(x => users.Select(u => u.Id).Contains(x.CustomerId) &&
                                                                x.Doctor.ClinicId == currentUserService.ClinicId &&
                                                                /* x.Date == DateOnly.FromDateTime(currentTime) &&*/
                                                                x.StartTime != null)
            .Include(x => x.Service)
            .Include(x => x.Doctor)
            .ThenInclude(d => d.User)
            .Include(x => x.ProcedurePriceType)
            .Include(x => x.Order)
            .OrderBy(x => x.StartTime);

        var pagedCustomerSchedules = await PagedResult<CustomerSchedule>.CreateAsync(
            query,
            request.PageIndex,
            request.PageSize
        );

        if (pagedCustomerSchedules.Items.Count == 0)
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("404", "No customer schedules found"));

        var userDict = users.ToDictionary(u => u.Id);

        var responses = pagedCustomerSchedules.Items
            .Select(schedule => MapToResponse(schedule, userDict))
            .ToList();

        var result = new PagedResult<Response.StaffCheckInCustomerScheduleResponse>(
            responses,
            pagedCustomerSchedules.PageIndex,
            pagedCustomerSchedules.PageSize,
            pagedCustomerSchedules.TotalCount
        );

        return Result.Success(result);
    }

    private static Expression<Func<User, bool>> CreateUserFilter(Query.StaffCheckInCustomerScheduleQuery request)
    {
        // Split customer name into potential first and last names
        var nameParts = request.CustomerName.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        // If phone is provided, include it in the filter
        if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
            return u => nameParts.Any(part =>
                (u.FirstName.Contains(part) || u.LastName.Contains(part)) &&
                u.PhoneNumber.Contains(request.CustomerPhone));

        // Filter only by name parts
        return u => nameParts.Any(part =>
            u.FirstName.Contains(part) || u.LastName.Contains(part));
    }

    private static Response.StaffCheckInCustomerScheduleResponse MapToResponse(
        CustomerSchedule schedule,
        Dictionary<Guid, User> userDict)
    {
        // Null checks and safe navigation
        var user = userDict.TryGetValue(schedule.CustomerId, out var foundUser)
            ? foundUser
            : throw new InvalidOperationException("User not found for schedule");

        return new Response.StaffCheckInCustomerScheduleResponse(
            schedule.Id,
            schedule.OrderId.Value,
            schedule.Order?.TotalAmount ?? 0,
            schedule.Order?.Discount ?? 0,
            schedule.Order?.DepositAmount ?? 0,
            schedule.Order?.FinalAmount ?? 0,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            schedule.DoctorNote,
            user.PhoneNumber,
            schedule.Service?.Name ?? string.Empty,
            schedule.Doctor?.User is not null
                ? $"{schedule.Doctor.User.FirstName} {schedule.Doctor.User.LastName}".Trim()
                : string.Empty,
            schedule.Date,
            schedule.StartTime,
            schedule.EndTime,
            schedule.Status,
            schedule.ProcedurePriceType?.Name ?? string.Empty,
            schedule.ProcedurePriceType?.Procedure.Name ?? string.Empty,
            schedule.ProcedurePriceType?.Procedure.StepIndex.ToString(),
            schedule.ProcedurePriceType.Procedure.StepIndex == 1
        );
    }
}