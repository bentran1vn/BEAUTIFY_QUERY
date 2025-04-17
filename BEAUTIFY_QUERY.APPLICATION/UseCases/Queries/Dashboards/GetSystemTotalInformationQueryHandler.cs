using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Dashboards;
public class GetSystemTotalInformationQueryHandler : IQueryHandler<Query.GetSystemTotalInformationQuery,
    Responses.GetSystemTotalInformationResponse>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;
    private readonly IRepositoryBase<Service, Guid> _serviceRepository;
    private readonly IRepositoryBase<UserClinic, Guid> _userClinicRepository;

    public GetSystemTotalInformationQueryHandler(IRepositoryBase<Clinic, Guid> clinicRepository,
        IRepositoryBase<UserClinic, Guid> userClinicRepository,
        IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository,
        IRepositoryBase<Service, Guid> serviceRepository)
    {
        _clinicRepository = clinicRepository;
        _userClinicRepository = userClinicRepository;
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<Responses.GetSystemTotalInformationResponse>> Handle(
        Query.GetSystemTotalInformationQuery request, CancellationToken cancellationToken)
    {
        var result = new Responses.GetSystemTotalInformationResponse();

        if (request.RoleName == "System Admin")
        {
            var query = _clinicRepository
                .FindAll(x => !x.IsDeleted);

            result.TotalClinics = await query.CountAsync(x => true, cancellationToken);
            result.TotalBranding = await query.CountAsync(x => x.IsParent == true, cancellationToken);
            result.TotalBranches = await query.CountAsync(x => x.IsParent == false, cancellationToken);
            result.TotalBranchActive = await query.CountAsync(x =>
                x.IsParent == false && x.IsActivated == true, cancellationToken);
            result.TotalBranchInActive = await query.CountAsync(x =>
                x.IsParent == false && x.IsActivated != true, cancellationToken);
        }

        var requestQuery = _clinicOnBoardingRequestRepository
            .FindAll(x => x.IsDeleted == false);

        result.TotalBrandPending = await requestQuery
            .Where(x =>
                !x.IsDeleted
                && x.Status == 0)
            .Select(x => x.ClinicId)
            .Distinct()
            .CountAsync(cancellationToken);

        var serviceQuery = _serviceRepository.FindAll(x => x.IsDeleted == false);

        result.TotalService = await serviceQuery.CountAsync(x => true, cancellationToken);

        var doctorQuery = _userClinicRepository
            .FindAll(x =>
                x.IsDeleted == false &&
                x.User.Role.Name.Equals("Doctor")
            ).GroupBy(x => x.UserId);

        result.TotalDoctor = await doctorQuery.CountAsync(x => true, cancellationToken);

        return Result.Success(result);
    }
}