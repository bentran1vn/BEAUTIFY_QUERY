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

    public GetDaytimeInformationQueryHandler(IRepositoryBase<Order, Guid> orderRepository, IRepositoryBase<CustomerSchedule, Guid> customerScheduleRepository, IRepositoryBase<ClinicService, Guid> clinicServiceRepository)
    {
        _orderRepository = orderRepository;
        _customerScheduleRepository = customerScheduleRepository;
        _clinicServiceRepository = clinicServiceRepository;
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
            serviceIds = await _clinicServiceRepository
                .FindAll(x => x.Clinics.ParentId.Equals(request.ClinicId))
                .Select(x => x.ServiceId)
                .ToListAsync(cancellationToken);
        }
        else if(request.RoleName == "Clinic Staff")
        {
            serviceIds = await _clinicServiceRepository
                .FindAll(x => x.ClinicId.Equals(request.ClinicId))
                .Select(x => x.ServiceId)
                .ToListAsync(cancellationToken);
        }
        
        orderQuery = orderQuery.Where(
            x => serviceIds.Contains((Guid)x.ServiceId!));
        
        var result = new Response.GetDaytimeInformationResponse();
        
        if (request.Date == null)
        {
            var infor = new Response.DatetimeInformation();
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
        }
        else
        {
            orderQuery = orderQuery.Where(x => x.OrderDate >= request.StartDate && x.OrderDate <= request.EndDate);
        }


        return Result.Success(result);
    }
}