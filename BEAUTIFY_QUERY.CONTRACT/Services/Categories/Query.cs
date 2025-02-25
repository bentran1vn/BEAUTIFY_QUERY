using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Categories;

public class Query
{
    public record GetAllCategoriesQuery() : IQuery<List<Response.GetAllCategories>>;
    public record GetAllCategoriesPagingQuery(int PageIndex, int PageSize) : IQuery<PagedResult<Response.GetAllCategories>>;
    public record GetCategoryByIdQuery(Guid Id) : IQuery<Response.GetAllCategories>;
}