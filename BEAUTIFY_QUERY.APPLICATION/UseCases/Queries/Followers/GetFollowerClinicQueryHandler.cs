using BEAUTIFY_QUERY.CONTRACT.Services.Followers;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Followers;

public class GetFollowerClinicQueryHandler: IQueryHandler<Query.GetFollowerClinicQuery, Response.GetFollowerClinic>
{
    private readonly IRepositoryBase<Follower, Guid> _followerRepository;
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;

    public GetFollowerClinicQueryHandler(IRepositoryBase<Follower, Guid> followerRepository, IRepositoryBase<Clinic, Guid> clinicRepository)
    {
        _followerRepository = followerRepository;
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<Response.GetFollowerClinic>> Handle(Query.GetFollowerClinicQuery request, CancellationToken cancellationToken)
    {
        var clinic = await _clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);
        
        if(clinic is null || clinic.IsDeleted)
            return Result.Failure<Response.GetFollowerClinic>(new Error("404", "Clinic not found"));
        
        if(clinic.IsParent != null && clinic.IsParent == false)
            return Result.Failure<Response.GetFollowerClinic>(new Error("400", "Clinic is not a parent clinic"));
        
        var query = _followerRepository
            .FindAll(x => x.ClinicId == request.ClinicId && !x.IsDeleted);

        var total = await query.CountAsync(cancellationToken);
        var isFollower = request.UserId != null && await query.AnyAsync(x => x.UserId == request.UserId, cancellationToken);
        
        var result = new Response.GetFollowerClinic(total, isFollower);
        
        return Result.Success(result);
    }
}