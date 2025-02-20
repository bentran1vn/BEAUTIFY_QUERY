using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Categories;

public class Query
{
    public record GetAllCategoriesQuery() : IQuery<List<Response.GetAllCategories>>;
    public record GetCategoryByIdQuery(Guid Id) : IQuery<Response.GetAllCategories>;
}