using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Users;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Users;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/users";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Users")
            .MapGroup(BaseUrl).HasApiVersion(1);
        
        gr1.MapGet("get-user-by-phone-or-email/{searchTerm}", GetUserByPhoneOrEmail);

        gr1.MapGet("information", GetUserInformation)
            .RequireAuthorization(Constant.Policy.POLICY_DOCTOR_AND_CUSTOMER);
        
        gr1.MapGet("{userId:guid}/balance", GetCustomerCurrentBalance).RequireAuthorization(Constant.Role.CLINIC_STAFF);

        gr1.MapGet("", GetUser)
            .RequireAuthorization(Constant.Policy.POLICY_SYSTEM_ADMIN);
    }
    
    private static async Task<IResult> GetUser(ISender sender, 
        string? searchTerm = null, int pageIndex = 0, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetUsersQuery(searchTerm, pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetCustomerCurrentBalance(ISender sender, Guid userId)
    {
        var result = await sender.Send(new Query.GetCustomerCurrentBalance(userId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetUserByPhoneOrEmail(ISender sender, string searchTerm)
    {
        var result = await sender.Send(new Query.GetUserByPhoneOrEmail(searchTerm));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetUserInformation(ISender sender)
    {
        var result = await sender.Send(new Query.GetUserInformation());
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}