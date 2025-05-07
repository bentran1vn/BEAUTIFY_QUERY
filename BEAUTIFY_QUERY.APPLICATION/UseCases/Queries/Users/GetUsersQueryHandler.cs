using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using Response = BEAUTIFY_QUERY.CONTRACT.Services.Orders.Response;
using User = BEAUTIFY_QUERY.DOMAIN.Entities.User;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Users;

public class GetUsersQueryHandler: IQueryHandler<Query.GetUsersQuery, PagedResult<Response.UserQueryResponse>>
{
    private readonly IRepositoryBase<User, Guid> _userRepository;

    public GetUsersQueryHandler(IRepositoryBase<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<Response.UserQueryResponse>>> Handle(Query.GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.FindAll(x => x.IsDeleted == false);

        query = request.SearchTerm != null
            ? query.Where(
                x => x.Email.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()) ||
                     x.FirstName.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()) ||
                     x.LastName.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()) ||
                     (x.PhoneNumber != null && x.PhoneNumber.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()))
            )
            : query;

        var page = await PagedResult<User>.CreateAsync(query, request.PageIndex, request.PageSize);

        var userResponse = page.Items.Select(x => new Response.UserQueryResponse(
                x.Id,
                x.Email,
                x.FirstName,
                x.LastName,
                x.FullName,
                x.DateOfBirth,
                x.PhoneNumber,
                x.Balance,
                x.ProfilePicture,
                x.City,
                x.District,
                x.Ward,
                x.Address,
                x.FullAddress,
                x.Status == 1,
                x.CreatedOnUtc
            )
        ).ToList();

        var result =
            PagedResult<Response.UserQueryResponse>.Create(userResponse, page.PageIndex, page.PageSize,
                page.TotalCount);

        return Result.Success(result);
    }
}