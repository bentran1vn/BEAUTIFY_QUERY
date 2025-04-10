using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using Microsoft.AspNetCore.Mvc;

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