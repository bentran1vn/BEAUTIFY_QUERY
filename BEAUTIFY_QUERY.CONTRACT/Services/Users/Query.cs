namespace BEAUTIFY_QUERY.CONTRACT.Services.Users;
public class Query
{
    public record GetUserByPhoneOrEmail(string PhoneOrEmail) : IQuery<Response.GetUserByPhoneAndEmailResponse>;
    public record GetUserInformation : IQuery<Response.GetUserInformationResponse>;
    public record GetCustomerCurrentBalance(Guid UserId) : IQuery<string>;
    public record GetUsersQuery(
        string? SearchTerm,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Orders.Response.UserQueryResponse>>;
}