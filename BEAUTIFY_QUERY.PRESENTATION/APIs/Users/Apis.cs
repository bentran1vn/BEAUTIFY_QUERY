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
    }

    private static async Task<IResult> GetUserByPhoneOrEmail(ISender sender, string searchTerm)
    {
        var result = await sender.Send(new Query.GetUserByPhoneOrEmail(searchTerm));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}