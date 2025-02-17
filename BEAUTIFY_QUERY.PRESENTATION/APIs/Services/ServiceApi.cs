using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Extensions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Query = BEAUTIFY_QUERY.CONTRACT.Services.Services.Query;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Services;

public class ServiceApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/services";
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Services")
            .MapGroup(BaseUrl).HasApiVersion(1);
        
        gr1.MapGet(string.Empty, GetAllServices);
        gr1.MapGet("{id}", GetServicesById);
    }
    
    private static async Task<IResult> GetAllServices(
        ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetClinicServicesQuery(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetServicesById(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetClinicServicesByIdQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}