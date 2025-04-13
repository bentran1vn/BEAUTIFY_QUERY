using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Dashboards;

public class GetDaytimeInformationQueryHandler : IQueryHandler<Query.GetDaytimeInformationQuery, Response.GetDaytimeInformationResponse>
{
    private readonly IRepositoryBase<Order, Guid> _orderRepository;
    private readonly IRepositoryBase<CustomerSchedule, Guid> _customerScheduleRepository;
    private readonly IRepositoryBase<ClinicService, Guid> _clinicServiceRepository;
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;

    public GetDaytimeInformationQueryHandler(IRepositoryBase<Order, Guid> orderRepository, IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepository, IRepositoryBase<ClinicService, Guid> clinicServiceRepository, IRepositoryBase<Clinic, Guid> clinicRepository)
    {
        _orderRepository = orderRepository;
        _customerScheduleRepository = customerScheduleRepository;
        _clinicServiceRepository = clinicServiceRepository;
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<Response.GetDaytimeInformationResponse>> Handle(Query.GetDaytimeInformationQuery request, CancellationToken cancellationToken)
    {
        if(request.StartDate == null || request.EndDate == null)
            return Result.Failure<Response.GetDaytimeInformationResponse>(new Error("400", "Start date and end date cannot be null"));

        var orderQuery = _orderRepository
            .FindAll(x => !x.IsDeleted);
        
        var customerScheduleQuery = _customerScheduleRepository
            .FindAll(x => !x.IsDeleted);

        if (request.Date == null)
        {
            orderQuery = orderQuery.Where(x => x.OrderDate.Equals(request.Date));
            customerScheduleQuery = customerScheduleQuery.Where(x => x.Date.Equals(request.Date));
        }
        else
        {
            orderQuery = orderQuery.Where(x => x.OrderDate >= request.StartDate && x.OrderDate <= request.EndDate);
            customerScheduleQuery = customerScheduleQuery.Where(x => x.Date >= request.StartDate && x.Date <= request.EndDate);
        }
        
        List<Guid> serviceIds = new();
        
        if(request.RoleName == "Clinic Admin")
        {
            var clinicIds = _clinicRepository
                .FindAll(x => x.ParentId.Equals(request.ClinicId))
                .Select(x => x.Id)
                .ToList();
            
            clinicIds.Add(request.ClinicId);
            
            orderQuery = orderQuery.Where(
                x => clinicIds.Contains(x.ClinicId));
        }
        else if(request.RoleName == "Clinic Staff")
        {
            orderQuery = orderQuery.Where(
                x => x.ClinicId.Equals(request.ClinicId));
        }
        
        var result = new Response.GetDaytimeInformationResponse();
        
        if (request.Date == null)
        {
            var infor = new Response.Information();
            infor.TotalSumRevenue = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0;
            infor.TotalSumRevenueNormal = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .Where(x => x.LivestreamRoomId == null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0;
            infor.TotalCountOrderPending = await orderQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync( cancellationToken);
            infor.TotalSumRevenueLiveStream = await orderQuery
                .Where(x => x.LivestreamRoomId != null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0;
            infor.TotalCountOrderCustomer = await orderQuery
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken);
            infor.TotalCountScheduleCustomer = await customerScheduleQuery
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken);
            infor.TotalCountCustomerSchedule = await customerScheduleQuery
                .CountAsync(cancellationToken);
            infor.TotalCountCustomerSchedulePending = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync(cancellationToken);
            infor.TotalCountCustomerScheduleInProgress = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_IN_PROGRESS)
                .CountAsync(cancellationToken);
            infor.TotalCountCustomerScheduleCompleted = await customerScheduleQuery
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .CountAsync(cancellationToken);
            
            result.DatetimeInformation = infor;
        }
        else
        {
            var listInfor = new List<Response.DatetimeInformation>();
            if (request.IsDisplayWeek == true)
            {
                DateOnly startDate = request.StartDate.Value;
                DateOnly endDate = request.EndDate.Value;
            
                // Find the first Monday in or before the start date to begin our week
                int daysToSubtract = (int)startDate.DayOfWeek - 1;
                if (daysToSubtract < 0) daysToSubtract += 7; // If it's Sunday (0), we want to go back 6 days
            
                DateOnly weekStart = startDate.AddDays(-daysToSubtract);
                
                while (weekStart <= endDate)
                {
                    // Calculate the end of the week (Sunday)
                    DateOnly weekEnd = weekStart.AddDays(6);
                
                    // If the week extends beyond the requested end date, cap it
                    if (weekEnd > endDate)
                        weekEnd = endDate;
                
                    var weekInfo = new Response.DatetimeInformation
                    {
                        StartDate = weekStart,
                        EndDate = weekEnd,
                        Information = await GetInformationForDateRange(orderQuery, customerScheduleQuery, weekStart, weekEnd, cancellationToken)
                    };
                
                    listInfor.Add(weekInfo);
                
                    // Move to next week
                    weekStart = weekStart.AddDays(7);
                }
            }
            else
            {
                // Display by Month
                DateOnly startDate = request.StartDate.Value;
                DateOnly endDate = request.EndDate.Value;
            
                // Start from the first day of the start month
                DateOnly monthStart = new DateOnly(startDate.Year, startDate.Month, 1);
            
                while (monthStart <= endDate)
                {
                    // Calculate the end of the month
                    DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                    // If the month extends beyond the requested end date, cap it
                    if (monthEnd > endDate)
                        monthEnd = endDate;
                
                    var monthInfo = new Response.DatetimeInformation
                    {
                        StartDate = monthStart,
                        EndDate = monthEnd,
                        Information = await GetInformationForDateRange(orderQuery, customerScheduleQuery, monthStart, monthEnd, cancellationToken)
                    };
                
                    listInfor.Add(monthInfo);
                
                    // Move to next month
                    monthStart = monthStart.AddMonths(1);
                }
            }
            result.DatetimeInformationList = listInfor;
        }
        return Result.Success(result);
    }
    
    private async Task<Response.Information> GetInformationForDateRange(
    IQueryable<Order> orderQuery, 
    IQueryable<CustomerSchedule> customerScheduleQuery,
    DateOnly startDate,
    DateOnly endDate,
    CancellationToken cancellationToken)
    {
        // Filter queries for the specific date range
        var ordersInRange = orderQuery.Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate);
        var schedulesInRange = customerScheduleQuery.Where(x => x.Date >= startDate && x.Date <= endDate);
        
        var info = new Response.Information
        {
            TotalSumRevenue = await ordersInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,
                
            TotalSumRevenueNormal = await ordersInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .Where(x => x.LivestreamRoomId == null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,
                
            TotalCountOrderPending = await ordersInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync(cancellationToken),
                
            TotalSumRevenueLiveStream = await ordersInRange
                .Where(x => x.LivestreamRoomId != null)
                .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0,
                
            TotalCountOrderCustomer = await ordersInRange
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken),
                
            TotalCountScheduleCustomer = await schedulesInRange
                .Select(x => x.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken),
                
            TotalCountCustomerSchedule = await schedulesInRange
                .CountAsync(cancellationToken),
                
            TotalCountCustomerSchedulePending = await schedulesInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_PENDING)
                .CountAsync(cancellationToken),
                
            TotalCountCustomerScheduleInProgress = await schedulesInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_IN_PROGRESS)
                .CountAsync(cancellationToken),
                
            TotalCountCustomerScheduleCompleted = await schedulesInRange
                .Where(x => x.Status == Constant.OrderStatus.ORDER_COMPLETED)
                .CountAsync(cancellationToken)
        };
        
        return info;
    }
}