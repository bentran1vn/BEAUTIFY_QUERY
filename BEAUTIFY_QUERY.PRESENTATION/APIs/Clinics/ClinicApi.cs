using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Clinics;

public class ClinicApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/user";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Clinics")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet("", CreateUser);

    }

    private static async Task<IResult> CreateUser(ISender sender)
    {
        return Results.Ok("Hello");
    }
}