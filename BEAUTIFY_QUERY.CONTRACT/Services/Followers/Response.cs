namespace BEAUTIFY_QUERY.CONTRACT.Services.Followers;

public class Response
{
    public class GetFollowerResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTimeOffset FollowDate { get; set; }
    }

    public record GetFollowerClinic(int TotalFollowers, bool IsFollowed);
}