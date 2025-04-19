using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using User = BEAUTIFY_QUERY.DOMAIN.Entities.User;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Users;
internal sealed class GetUserByPhoneOrEmailQueryHandler(IRepositoryBase<User, Guid> userRepositoryBase)
    : IQueryHandler<Query.GetUserByPhoneOrEmail, Response.GetUserByPhoneAndEmailResponse>
{
    public async Task<Result<Response.GetUserByPhoneAndEmailResponse>> Handle(
        Query.GetUserByPhoneOrEmail request, CancellationToken cancellationToken)
    {
        // Normalize the input by trimming whitespace
        var normalizedInput = request.PhoneOrEmail?.Trim() ?? string.Empty;

        // Check if the input is a valid email address
        var isEmail = normalizedInput.Contains("@") && normalizedInput.Contains(".");

        // Query the database based on whether the input is an email or phone number
        var user = await userRepositoryBase.FindSingleAsync(
            x => (isEmail ? x.Email == normalizedInput : x.PhoneNumber == normalizedInput) && !x.IsDeleted,
            cancellationToken);

        // If no user is found, return a failure result
        return user is null
            ? Result.Failure<Response.GetUserByPhoneAndEmailResponse>(new Error("404", "User Not Found !"))
            :
            // Map the user entity to the response DTO and return it
            Result.Success(new Response.GetUserByPhoneAndEmailResponse(user.Id));
    }
}