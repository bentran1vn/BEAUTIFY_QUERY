using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Categories;
public class GetCategoryByIdQueryHandler(
    IRepositoryBase<Category, Guid> categoryRepository)
    : IQueryHandler<Query.GetCategoryByIdQuery, Response.GetAllCategoriesWithSubCategories>
{
    public async Task<Result<Response.GetAllCategoriesWithSubCategories>> Handle(Query.GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.FindByIdAsync(request.Id, cancellationToken, x => x.Children);

        if (category == null || category.IsDeleted)
            return Result.Failure<Response.GetAllCategoriesWithSubCategories>(new Error("404", "Category not found"));

        var subCategories = category.Children?.Select(MapToResponse).ToList() ?? [];

        var result = new Response.GetAllCategoriesWithSubCategories(
            category.Id,
            category.Name,
            category.Description ?? "",
            category.IsParent,
            category.ParentId,
            category.IsDeleted,
            subCategories
        );

        return Result.Success(result);
    }

    private static Response.GetAllCategories MapToResponse(Category category)
    {
        return new Response.GetAllCategories(
            category.Id,
            category.Name,
            category.Description ?? "",
            category.IsParent,
            category.ParentId,
            category.IsDeleted
        );
    }
}