namespace BEAUTIFY_QUERY.CONTRACT.Services.Users;
public static class Response
{
    public record GetUserByPhoneAndEmailResponse(Guid Id);

    public record GetUserInformationResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string FullName,
        DateOnly? DateOfBirth,
        string Email,
        string? Phone,
        string? ProfilePicture,
        string? City,
        string? District,
        string? Ward,
        string? Address,
        string? FullAddress,
        decimal? Balance = 0
    );
}