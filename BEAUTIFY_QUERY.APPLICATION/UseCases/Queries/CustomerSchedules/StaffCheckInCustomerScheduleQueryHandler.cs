using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.CustomerSchedules;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

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
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("400", "Customer name is required"));

        // Create a dynamic user filter based on available information
        var userFilter = CreateUserFilter(request);

        // Fetch users with a single database query
        var users = await userRepositoryBase.FindAll(userFilter)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("404", "No matching users found"));

        var VietNameTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietNameTimeZone);


        // Create query for customer schedules with includes
        var query = customerScheduleRepositoryBase.FindAll(
                x => users.Select(u => u.Id).Contains(x.CustomerId) &&
                     x.Doctor.ClinicId == currentUserService.ClinicId &&
                     /* x.Date == DateOnly.FromDateTime(currentTime) &&*/ x.StartTime != null)
            .Include(x => x.Service)
            .Include(x => x.Doctor)
            .ThenInclude(d => d.User)
            .Include(x => x.ProcedurePriceType)
            .Include(x => x.Order)
            .OrderBy(x => x.StartTime);

        // Apply pagination
        var pagedCustomerSchedules = await PagedResult<CustomerSchedule>.CreateAsync(
            query,
            request.PageIndex,
            request.PageSize
        );

        if (pagedCustomerSchedules.Items.Count == 0)
            return Result.Failure<PagedResult<Response.StaffCheckInCustomerScheduleResponse>>(
                new Error("404", "No customer schedules found"));

        // Create a dictionary for quick user lookup
        var userDict = users.ToDictionary(u => u.Id);

        // Map to response with null-safe navigation and optimized projection
        var responses = pagedCustomerSchedules.Items
            .Select(schedule => MapToResponse(schedule, userDict))
            .ToList();

        // Create paged result with mapped items
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
        {
            return u => nameParts.Any(part =>
                (u.FirstName.Contains(part) || u.LastName.Contains(part)) &&
                u.PhoneNumber.Contains(request.CustomerPhone));
        }

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
            Id: schedule.Id,
            OrderId: schedule.OrderId.Value,
            ServicePrice: schedule.Order?.TotalAmount ?? 0,
            DiscountAmount: schedule.Order?.Discount ?? 0,
            DepositAmount: schedule.Order?.DepositAmount ?? 0,
            Amount: schedule.Order?.FinalAmount ?? 0,
            CustomerName: $"{user.FirstName} {user.LastName}".Trim(),
            CustomerEmail: user.Email,
            CustomerPhoneNumber: user.PhoneNumber,
            ServiceName: schedule.Service?.Name ?? string.Empty,
            DoctorName: schedule.Doctor?.User is not null
                ? $"{schedule.Doctor.User.FirstName} {schedule.Doctor.User.LastName}".Trim()
                : string.Empty,
            BookingDate: schedule.Date,
            StartTime: schedule.StartTime,
            EndTime: schedule.EndTime,
            Status: schedule.Status,
            ProcedurePriceTypeName: schedule.ProcedurePriceType?.Name ?? string.Empty,
            ProcedureName: schedule.ProcedurePriceType?.Procedure.Name ?? string.Empty,
            StepIndex: schedule.ProcedurePriceType?.Procedure.StepIndex.ToString(),
            schedule.ProcedurePriceType.Procedure.StepIndex == 1
        );
    }
}