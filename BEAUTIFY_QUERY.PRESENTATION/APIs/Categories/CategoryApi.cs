using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Categories;

public class CategoryApi: ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/categories";
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Categories")
            .MapGroup(BaseUrl).HasApiVersion(1);
        
        gr1.MapGet(string.Empty, GetAllCategories);
    }
    
    private static async Task<IResult> GetAllCategories(
        ISender sender)
    {
        var result = await sender.Send(new Query.GetAllCategoriesQuery());
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}