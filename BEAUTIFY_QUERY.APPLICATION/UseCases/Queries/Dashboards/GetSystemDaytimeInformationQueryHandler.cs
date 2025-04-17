using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Dashboards;
public class GetSystemDaytimeInformationQueryHandler : IQueryHandler<Query.GetSystemDaytimeInformationQuery,
    Responses.GetSystemDaytimeInformationResponse>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;
    private readonly IRepositoryBase<ClinicTransaction, Guid> _clinicTransactionRepository;
    private readonly IRepositoryBase<UserClinic, Guid> _doctorReRepository;
    private readonly IRepositoryBase<Order, Guid> _orderRepository;
    private readonly IRepositoryBase<Service, Guid> _serviceRepository;
    private readonly IRepositoryBase<SystemTransaction, Guid> _systemTransactionRepository;

    public GetSystemDaytimeInformationQueryHandler(
        IRepositoryBase<Clinic, Guid> clinicRepository,
        IRepositoryBase<Service, Guid> serviceRepository,
        IRepositoryBase<UserClinic, Guid> doctorReRepository,
        IRepositoryBase<SystemTransaction, Guid> systemTransactionRepository,
        IRepositoryBase<ClinicTransaction, Guid> clinicTransactionRepository,
        IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository,
        IRepositoryBase<Order, Guid> orderRepository)
    {
        _clinicRepository = clinicRepository;
        _serviceRepository = serviceRepository;
        _doctorReRepository = doctorReRepository;
        _systemTransactionRepository = systemTransactionRepository;
        _clinicTransactionRepository = clinicTransactionRepository;
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<Responses.GetSystemDaytimeInformationResponse>> Handle(
        Query.GetSystemDaytimeInformationQuery request, CancellationToken cancellationToken)
    {
        // Validate date parameters
        if ((request.StartDate == null && request.EndDate != null) ||
            (request.StartDate != null && request.EndDate == null))
            return Result.Failure<Responses.GetSystemDaytimeInformationResponse>(
                new Error("400", "Start date and end date must both be provided or both be null"));

        // Initialize base queries with deleted filter
        var systemTransactionQuery = _systemTransactionRepository.FindAll(x => !x.IsDeleted);
        var clinicTransactionQuery = _clinicTransactionRepository.FindAll(x => !x.IsDeleted);
        var clinicQuery = _clinicRepository.FindAll(x => !x.IsDeleted);
        var serviceQuery = _serviceRepository.FindAll(x => !x.IsDeleted);
        var doctorQuery = _doctorReRepository.FindAll(x => !x.IsDeleted);
        var clinicOnBoardingRequestQuery = _clinicOnBoardingRequestRepository.FindAll(x => !x.IsDeleted);
        var orderQuery = _orderRepository.FindAll(x => !x.IsDeleted);

        var result = new Responses.GetSystemDaytimeInformationResponse();

        // Handle single date request
        if (request.Date != null)
        {
            // Apply date filters to all queries
            systemTransactionQuery = systemTransactionQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            clinicTransactionQuery = clinicTransactionQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            clinicQuery = clinicQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            serviceQuery = serviceQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            doctorQuery = doctorQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            clinicOnBoardingRequestQuery = clinicOnBoardingRequestQuery.Where(x => x.CreatedOnUtc.Equals(request.Date));
            orderQuery = orderQuery.Where(x => x.OrderDate.Equals(request.Date));

            // Get information for single date
            result.SystemInformation = await GetInformationForQueries(
                clinicQuery,
                serviceQuery,
                clinicOnBoardingRequestQuery,
                doctorQuery,
                systemTransactionQuery,
                clinicTransactionQuery,
                orderQuery,
                cancellationToken);
        }
        else if (request.StartDate != null && request.EndDate != null)
        {
            // Apply date range filters to all queries
            systemTransactionQuery = systemTransactionQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            clinicTransactionQuery = clinicTransactionQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            clinicQuery = clinicQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            serviceQuery = serviceQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            doctorQuery = doctorQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            clinicOnBoardingRequestQuery = clinicOnBoardingRequestQuery
                .Where(x => DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= request.StartDate &&
                            DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= request.EndDate);

            orderQuery = orderQuery
                .Where(x => x.OrderDate >= request.StartDate && x.OrderDate <= request.EndDate);

            var listInfor = new List<Responses.SystemDatetimeInformation>();
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
                    var systemTransactionsInWeek = systemTransactionQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var clinicTransactionsInWeek = clinicTransactionQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var clinicsInWeek = clinicQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var servicesInWeek = serviceQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var doctorsInWeek = doctorQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var clinicOnBoardingRequestsInWeek = clinicOnBoardingRequestQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= weekStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= weekEnd);

                    var ordersInWeek = orderQuery.Where(x =>
                        x.OrderDate >= weekStart &&
                        x.OrderDate <= weekEnd);

                    var weekInfo = new Responses.SystemDatetimeInformation
                    {
                        StartDate = weekStart,
                        EndDate = weekEnd,
                        Information = await GetInformationForQueries(
                            clinicsInWeek,
                            servicesInWeek,
                            clinicOnBoardingRequestsInWeek,
                            doctorsInWeek,
                            systemTransactionsInWeek,
                            clinicTransactionsInWeek,
                            ordersInWeek,
                            cancellationToken)
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
                    var systemTransactionsInMonth = systemTransactionQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var clinicTransactionsInMonth = clinicTransactionQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var clinicsInMonth = clinicQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var servicesInMonth = serviceQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var doctorsInMonth = doctorQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var clinicOnBoardingRequestsInMonth = clinicOnBoardingRequestQuery.Where(x =>
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) >= monthStart &&
                        DateOnly.FromDateTime(x.CreatedOnUtc.Date) <= monthEnd);

                    var ordersInMonth = orderQuery.Where(x =>
                        x.OrderDate >= monthStart &&
                        x.OrderDate <= monthEnd);

                    var monthInfo = new Responses.SystemDatetimeInformation
                    {
                        StartDate = monthStart,
                        EndDate = monthEnd,
                        Information = await GetInformationForQueries(
                            clinicsInMonth,
                            servicesInMonth,
                            clinicOnBoardingRequestsInMonth,
                            doctorsInMonth,
                            systemTransactionsInMonth,
                            clinicTransactionsInMonth,
                            ordersInMonth,
                            cancellationToken)
                    };

                    listInfor.Add(monthInfo);
                    monthStart = monthStart.AddMonths(1);
                }
            }

            result.SystemInformationList = listInfor;
        }

        return Result.Success(result);
    }

    private async Task<Responses.SystemInformation> GetInformationForQueries(
        IQueryable<Clinic> clinicQuery,
        IQueryable<Service> serviceQuery,
        IQueryable<ClinicOnBoardingRequest> clinicOnBoardingRequestQuery,
        IQueryable<UserClinic> doctorQuery,
        IQueryable<SystemTransaction> systemTransactionQuery,
        IQueryable<ClinicTransaction> clinicTransactionQuery,
        IQueryable<Order> orderQuery,
        CancellationToken cancellationToken)
    {
        var infor = new Responses.SystemInformation();

        infor.TotalCountBranding = await clinicQuery
            .CountAsync(x =>
                !x.IsDeleted
                && x.IsParent == true, cancellationToken);

        infor.TotalCountBranches = await clinicQuery
            .CountAsync(x =>
                !x.IsDeleted
                && x.IsParent == false, cancellationToken);

        infor.TotalCountService = await serviceQuery
            .CountAsync(x =>
                !x.IsDeleted, cancellationToken);

        infor.TotalCountDoctor = await doctorQuery
            .Where(x =>
                x.IsDeleted == false &&
                x.User.Role.Name.Equals("Doctor"))
            .GroupBy(x => x.UserId)
            .CountAsync(cancellationToken);

        infor.TotalCountBronzeSubscription = await systemTransactionQuery
            .CountAsync(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.AdditionBranches == null &&
                x.AdditionLivestreams == null &&
                x.SubscriptionPackage.Name.Equals("Đồng"), cancellationToken);

        infor.TotalCountSilverSubscription = await systemTransactionQuery
            .CountAsync(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.AdditionBranches == null &&
                x.AdditionLivestreams == null &&
                x.SubscriptionPackage.Name.Equals("Bạc"), cancellationToken);

        infor.TotalCountGoldSubscription = await systemTransactionQuery
            .CountAsync(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.AdditionBranches == null &&
                x.AdditionLivestreams == null &&
                x.SubscriptionPackage.Name.Equals("Vàng"), cancellationToken);

        infor.TotalSumGoldSubscriptionRevenue = await systemTransactionQuery
            .Where(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.SubscriptionPackage.Name.Equals("Vàng"))
            .SumAsync(x => x.Amount, cancellationToken);

        infor.TotalSumSilverSubscriptionRevenue = await systemTransactionQuery
            .Where(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.SubscriptionPackage.Name.Equals("Bạc"))
            .SumAsync(x => x.Amount, cancellationToken);

        infor.TotalSumBronzeSubscriptionRevenue = await systemTransactionQuery
            .Where(x =>
                !x.IsDeleted && x.Status == 1 &&
                x.SubscriptionPackage.Name.Equals("Đồng"))
            .SumAsync(x => x.Amount, cancellationToken);

        infor.TotalSumClinicRevenue = await orderQuery
            .Where(x =>
                !x.IsDeleted && x.Status == Constant.OrderStatus.ORDER_COMPLETED)
            .SumAsync(x => x.FinalAmount, cancellationToken) ?? 0;

        // Calculate total system revenue
        infor.TotalSystemSumRevenue = await systemTransactionQuery
            .Where(x => !x.IsDeleted && x.Status == 1)
            .SumAsync(x => x.Amount, cancellationToken);

        // Calculate total revenue (system + clinic)
        infor.TotalSumRevenue = infor.TotalSystemSumRevenue + infor.TotalSumClinicRevenue;

        return infor;
    }
}