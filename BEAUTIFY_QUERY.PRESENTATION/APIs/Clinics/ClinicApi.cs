using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Clinics;

public class ClinicApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/clinics";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Clinics")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet("apply", GetAllApplyRequest);
        gr1.MapGet("apply/{id}", GetDetailApplyRequest);
    }

    private static async Task<IResult> GetAllApplyRequest(ISender sender, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllApplyRequestQuery(pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    private static async Task<IResult> GetDetailApplyRequest(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetDetailApplyRequestQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}