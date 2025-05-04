using BEAUTIFY_QUERY.CONTRACT.Services.Followers;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Followers;

public class GetFollowerQueryHandler: IQueryHandler<Query.GetFollowerQuery, PagedResult<Response.GetFollowerResponse>>
{
    private readonly IRepositoryBase<Follower, Guid> _followerRepository;

    public GetFollowerQueryHandler(IRepositoryBase<Follower, Guid> followerRepository)
    {
        _followerRepository = followerRepository;
    }

    public async Task<Result<PagedResult<Response.GetFollowerResponse>>> Handle(Query.GetFollowerQuery request, CancellationToken cancellationToken)
    {
        var query = _followerRepository.FindAll(
            x => !x.IsDeleted && x.ClinicId == request.ClinicId);
        
        query = string.IsNullOrWhiteSpace(request.SearchTerm) 
            ? query
            : query.Where(x => x.User.FullName.ToLower().Contains(request.SearchTerm.ToLower())
                              || x.User.Email.ToLower().Contains(request.SearchTerm.ToLower())
                              || x.User.PhoneNumber.ToLower().Contains(request.SearchTerm.ToLower()));


        query = query.Include(x => x.User);
        
        var followers = await PagedResult<Follower>.CreateAsync(query, request.PageNumber, request.PageSize);
        
        var result = new PagedResult<Response.GetFollowerResponse>(followers.Items
                .Select(x => new Response.GetFollowerResponse()
                {
                    Id = x.Id,
                    UserId = (Guid)x.UserId,
                    FullName = x.User.FullName,
                    Email = x.User.Email,
                    PhoneNumber = x.User.PhoneNumber,
                    ProfilePictureUrl = x.User.ProfilePicture,
                    FollowDate = x.CreatedOnUtc
                })
                .ToList(),
            followers.PageIndex, followers.PageSize, followers.TotalCount);
        
        return Result.Success(result);
    }
}