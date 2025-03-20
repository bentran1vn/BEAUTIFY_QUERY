namespace BEAUTIFY_QUERY.CONTRACT.Services.Users;
public class Query
{
    public record GetUserByPhoneOrEmail(string PhoneOrEmail) : IQuery<Response.GetUserByPhoneAndEmailResponse>;
}