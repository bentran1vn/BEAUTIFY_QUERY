using BEAUTIFY_QUERY.CONTRACT.Services.Categories;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Categories;
public class CategoryApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/categories";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Categories")
            .MapGroup(BaseUrl).HasApiVersion(1);

        gr1.MapGet(string.Empty, GetAllCategories);
        gr1.MapGet("{id}", GetCategoryId);
    }

    private static async Task<IResult> GetAllCategories(
        ISender sender,
        string? searchTerm = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        Result result;
        if (pageIndex != null && pageSize == null) result = await sender.Send(new Query.GetAllCategoriesQuery());
        else
            result = await sender.Send(
                new Query.GetAllCategoriesPagingQuery(searchTerm, (int)pageIndex!, (int)pageSize!));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetCategoryId(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetCategoryByIdQuery(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}