using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Users;
internal sealed class GetUserInformationQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<Staff, Guid> repositoryBase)
    : IQueryHandler<Query.GetUserInformation, Response.GetUserInformationResponse>
{
    public async Task<Result<Response.GetUserInformationResponse>> Handle(Query.GetUserInformation request,
        CancellationToken cancellationToken)
    {
        var user = await repositoryBase.FindByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (user is null)
            return Result.Failure<Response.GetUserInformationResponse>(
                new Error("404", "User Not Found !"));
        var response = new Response.GetUserInformationResponse
        (
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.DateOfBirth,
            user.Email,
            user.PhoneNumber,
            user.ProfilePicture,
            user.City,
            user.District,
            user.Ward,
            user.Address,
            user.FullAddress
        );
        return Result.Success(response);
    }
}