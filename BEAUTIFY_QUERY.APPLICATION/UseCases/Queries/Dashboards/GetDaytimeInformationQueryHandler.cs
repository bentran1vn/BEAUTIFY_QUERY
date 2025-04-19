using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Dashboards;
public class
    GetDaytimeInformationQueryHandler : IQueryHandler<Query.GetDaytimeInformationQuery,
    Responses.GetDaytimeInformationResponse>
{
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;
    private readonly IRepositoryBase<ClinicService, Guid> _clinicServiceRepository;
    private readonly IRepositoryBase<ClinicTransaction, Guid> _clinicTransactionRepository;
    private readonly IRepositoryBase<CustomerSchedule, Guid> _customerScheduleRepository;
    private readonly IRepositoryBase<Order, Guid> _orderRepository;

    public GetDaytimeInformationQueryHandler(
        IRepositoryBase<Order, Guid> orderRepository,
        IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepository,
        IRepositoryBase<ClinicService, Guid> clinicServiceRepository,
        IRepositoryBase<Clinic, Guid> clinicRepository,
        IRepositoryBase<ClinicTransaction, Guid> clinicTransactionRepository)
    {
        _orderRepository = orderRepository;
        _customerScheduleRepository = customerScheduleRepository;
        _clinicServiceRepository = clinicServiceRepository;
        _clinicRepository = clinicRepository;
        _clinicTransactionRepository = clinicTransactionRepository;
    }

    public async Task<Result<Responses.GetDaytimeInformationResponse>> Handle(Query.GetDaytimeInformationQuery request,
        CancellationToken cancellationToken)
    {
        // Validate date parameters
        if ((request.StartDate == null && request.EndDate != null) ||
            (request.StartDate != null && request.EndDate == null))
            return Result.Failure<Responses.GetDaytimeInformationResponse>(
                new Error("400", "Start date and end date must both be provided or both be null"));

        // Base queries with deleted filter
        var orderQuery = _orderRepository.FindAll(x => !x.IsDeleted);
        var customerScheduleQuery = _customerScheduleRepository.FindAll(x => !x.IsDeleted);

        // Apply date filters
        if (request.Date != null)
        {
            orderQuery = orderQuery.Where(x => x.OrderDate.Equals(request.Date));
            customerScheduleQuery = customerScheduleQuery.Where(x => x.Date.Equals(request.Date));
        }
        else if (request.StartDate != null && request.EndDate != null)
        {
            orderQuery = orderQuery.Where(x => x.OrderDate >= request.StartDate && x.OrderDate <= request.EndDate);
            customerScheduleQuery =
                customerScheduleQuery.Where(x => x.Date >= request.StartDate && x.Date <= request.EndDate);
        }

        // Apply role-based filters
        const string ROLE_CLINIC_ADMIN = "Clinic Admin";
        const string ROLE_CLINIC_STAFF = "Clinic Staff";

        if (request.RoleName == ROLE_CLINIC_ADMIN)
        {
            var clinicIds = await _clinicRepository
                .FindAll(x => x.ParentId.Equals(request.ClinicId))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            clinicIds.Add(request.ClinicId);

            var orders = await _clinicTransactionRepository
                .FindAll(x => x.ClinicId.HasValue && clinicIds.Contains((Guid)x.ClinicId))
                .Select(x => x.OrderId)
                .ToListAsync(cancellationToken);

            orderQuery = orderQuery.Where(x => orders.Contains(x.Id));
        }
        else if (request.RoleName == ROLE_CLINIC_STAFF)
        {
            var orders = await _clinicTransactionRepository
                .FindAll(x => x.ClinicId.Equals(request.ClinicId))
                .Select(x => x.OrderId)
                .ToListAsync(cancellationToken);

            orderQuery = orderQuery.Where(x => orders.Contains(x.Id));
        }

        var result = new Responses.GetDaytimeInformationResponse();

        // Handle single date request
        if (request.Date != null)
        {
            result.DatetimeInformation = await GetInformationForQueries(
                orderQuery, customerScheduleQuery, cancellationToken);
        }
        else if (request.StartDate != null && request.EndDate != null)
        {
            var listInfor = new List<Responses.DatetimeInformation>();
            var startDate = request.StartDate.Value;
            var endDate = request.EndDate.Value;

            if (request.IsDisplayWeek == true)
            {
                // Find the first Monday in or before the start date
                var daysToSubtract = ((int)startDate.DayOfWeek - 1 + 7) % 7;
                var weekStart = startDate.AddDays(-daysToSubtract);

                while (weekStart <= endDate)
                {
                    // Calculate week end (Sunday)
                    var weekEnd = weekStart.AddDays(6);

                    // Only include complete weeks that fall within the range
                    if (weekEnd > endDate)
                        break;

                    // Filter queries for this specific week
                    var ordersInWeek = orderQuery.Where(x => x.OrderDate >= weekStart && x.OrderDate <= weekEnd);
                    var schedulesInWeek = customerScheduleQuery.Where(x => x.Date >= weekStart && x.Date <= weekEnd);

                    var weekInfo = new Responses.DatetimeInformation
                    {
                        StartDate = weekStart,
                        EndDate = weekEnd,
                        Information = await GetInformationForQueries(ordersInWeek, schedulesInWeek, cancellationToken)
                    };

                    listInfor.Add(weekInfo);
                    weekStart = weekStart.AddDays(7);
                }
            }
            else
            {
                // Display by Month - Start from the first day of the start month
                var monthStart = new DateOnly(startDate.Year, startDate.Month, 1);

                while (monthStart <= endDate)
                {
                    // Calculate the end of the month
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // Only include complete months that fall within the range
                    if (monthEnd > endDate)
                        break;

                    // Filter queries for this specific month
                    var ordersInMonth = orderQuery.Where(x => x.OrderDate >= monthStart && x.OrderDate <= monthEnd);
                    var schedulesInMonth = customerScheduleQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd);

                    var monthInfo = new Responses.DatetimeInformation
                    {
                        StartDate = monthStart,
                        EndDate = monthEnd,
                        Information = await GetInformationForQueries(ordersInMonth, schedulesInMonth, cancellationToken)
                    };

                    listInfor.Add(monthInfo);
                    monthStart = monthStart.AddMonths(1);
                }
            }

            result.DatetimeInformationList = listInfor;
        }

        return Result.Success(result);
    }

    // Combined the two information methods into one reusable method
    private async Task<Responses.Information> GetInformationForQueries(
        IQueryable<Order> orderQuery,
        IQueryable<CustomerSchedule> customerScheduleQuery,
        CancellationToken cancellationToken)
    {
        var info = new Responses.Information
        {
            TotalSumRevenue = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,

            TotalSumRevenueNormal = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED && x.LivestreamRoomId == null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,

            TotalCountOrderPending = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync(cancellationToken),

            TotalSumRevenueLiveStream = await orderQuery
                .Where(x => x.LivestreamRoomId != null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,

            TotalCountOrderCustomer = await orderQuery
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken),

            TotalCountScheduleCustomer = await customerScheduleQuery
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken),

            TotalCountCustomerSchedule = await customerScheduleQuery
                .CountAsync(cancellationToken),

            TotalCountCustomerSchedulePending = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync(cancellationToken),

            TotalCountCustomerScheduleInProgress = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_IN_PROGRESS)
                .CountAsync(cancellationToken),

            TotalCountCustomerScheduleCompleted = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .CountAsync(cancellationToken)
        };

        return info;
    }
}