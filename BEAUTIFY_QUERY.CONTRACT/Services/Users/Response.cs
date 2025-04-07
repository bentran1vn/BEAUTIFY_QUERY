namespace BEAUTIFY_QUERY.CONTRACT.Services.Users;
public static class Response
{
    public record GetUserByPhoneAndEmailResponse(Guid Id);

    public record GetUserInformationResponse(Guid Id, string FullName, string Email, string Phone, string ProfilePicture);
}