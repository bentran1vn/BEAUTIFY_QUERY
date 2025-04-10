using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Users;
internal sealed class GetUserInformationQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<Staff, Guid> repositoryBase,
    IRepositoryBase<User, Guid> userRepositoryBase)
    : IQueryHandler<Query.GetUserInformation, Response.GetUserInformationResponse>
{
    public async Task<Result<Response.GetUserInformationResponse>> Handle(
        Query.GetUserInformation request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<Response.GetUserInformationResponse>(
                new Error("400", "Invalid user identifier."));

        var userId = currentUserService.UserId.Value;
        var role = currentUserService.Role;

        return role == Constant.Role.DOCTOR
            ? await GetStaffResponse(userId, cancellationToken)
            : await GetUserResponse(userId, cancellationToken);
    }

    private async Task<Result<Response.GetUserInformationResponse>> GetStaffResponse(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var staff = await repositoryBase.FindByIdAsync(userId, cancellationToken);
        return staff is null
            ? Result.Failure<Response.GetUserInformationResponse>(
                new Error("404", "Staff not found."))
            : Result.Success(MapToResponse(staff));
    }

    private async Task<Result<Response.GetUserInformationResponse>> GetUserResponse(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userRepositoryBase.FindByIdAsync(userId, cancellationToken);
        return user is null
            ? Result.Failure<Response.GetUserInformationResponse>(
                new Error("404", "User not found."))
            : Result.Success(MapToResponse(user));
    }

    private static Response.GetUserInformationResponse MapToResponse<T>(T entity)
        where T : class
    {
        switch (entity)
        {
            case Staff staff:
                return new Response.GetUserInformationResponse(
                    staff.Id,
                    staff.FirstName,
                    staff.LastName,
                    staff.FullName,
                    staff.DateOfBirth,
                    staff.Email,
                    staff.PhoneNumber,
                    staff.ProfilePicture,
                    staff.City,
                    staff.District,
                    staff.Ward,
                    staff.Address,
                    staff.FullAddress);

            case User user:
                return new Response.GetUserInformationResponse(
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
                    user.FullAddress);

            default:
                throw new ArgumentException("Unsupported entity type.", nameof(entity));
        }
    }
}