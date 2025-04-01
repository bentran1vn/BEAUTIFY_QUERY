namespace BEAUTIFY_QUERY.CONTRACT.Services.Categories;
public class Query
{
    public record GetAllCategoriesQuery : IQuery<List<Response.GetAllCategories>>;

    public record GetAllCategoriesPagingQuery(string? searchTerm,int PageIndex, int PageSize)
        : IQuery<PagedResult<Response.GetAllCategories>>;

    public record GetCategoryByIdQuery(Guid Id) : IQuery<Response.GetAllCategoriesWithSubCategories>;
}