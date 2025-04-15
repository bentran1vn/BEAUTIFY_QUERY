using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Dashboards;

public class GetTotalInformationQueryHandler : IQueryHandler<Query.GetTotalInformationQuery, Responses.GetTotalInformationResponse>
{
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;
    private readonly IRepositoryBase<UserClinic, Guid> _userClinicRepository;
    private readonly IRepositoryBase<ClinicService, Guid> _clinicServiceRepository;

    public GetTotalInformationQueryHandler(IRepositoryBase<Clinic, Guid> clinicRepository, IRepositoryBase<UserClinic, Guid> userClinicRepository, IRepositoryBase<ClinicService, Guid> clinicServiceRepository)
    {
        _clinicRepository = clinicRepository;
        _userClinicRepository = userClinicRepository;
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result<Responses.GetTotalInformationResponse>> Handle(Query.GetTotalInformationQuery request, CancellationToken cancellationToken)
    {
        var result = new Responses.GetTotalInformationResponse();

        if (request.RoleName == "Clinic Admin")
        {
            var clinicQuery = _clinicRepository
                .FindAll(x => x.ParentId.Equals(request.ClinicId) && !x.IsDeleted);

            result.TotalBranch = await clinicQuery.CountAsync(cancellationToken);
            result.TotalBranchActive = await clinicQuery.CountAsync(x => x.IsActivated, cancellationToken);
            result.TotalBranchInActive = await clinicQuery.CountAsync(x => !x.IsActivated, cancellationToken);
        }
        
        var query =  _userClinicRepository
            .FindAll(x =>
                !x.IsDeleted 
            );

        if (request.RoleName == "Clinic Admin")
        {
            query = query.Where(x =>
                x.Clinic!.ParentId.Equals(request.ClinicId));
        } else if (request.RoleName == "Clinic Staff")
        {
            query = query.Where(x =>
                x.ClinicId.Equals(request.ClinicId));
        }

        query = query
            .Include(x => x.User)
            .ThenInclude(x => x.Role);
            
        var userClinic = await query.ToListAsync(cancellationToken);
            
        if (request.RoleName == "Clinic Admin")
        {
            result.TotalStaff = userClinic.Count(x => x.User!.Role!.Name == "Clinic Staff");
        }
        
        result.TotalDoctor = userClinic.Count(x => x.User!.Role!.Name == "Doctor");
        
        var serviceQuery = _clinicServiceRepository
            .FindAll(x => !x.IsDeleted);
        
        if (request.RoleName == "Clinic Admin")
        {
            serviceQuery = serviceQuery.Where(x =>
                x.Clinics!.ParentId.Equals(request.ClinicId));
        } else if (request.RoleName == "Clinic Staff")
        {
            serviceQuery = serviceQuery.Where(x => x.ClinicId.Equals(request.ClinicId));
        }
        
        result.TotalService = await serviceQuery.CountAsync(cancellationToken);

        return Result.Success(result);
    }
}