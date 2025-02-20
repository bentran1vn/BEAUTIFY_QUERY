using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Categories;

public class GetCategoryByIdQueryHandler: IQueryHandler<Query.GetCategoryByIdQuery,Response.GetAllCategories>
{
    private readonly IRepositoryBase<Category, Guid> _categoryRepository;

    public GetCategoryByIdQueryHandler(IRepositoryBase<Category, Guid> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<Response.GetAllCategories>> Handle(Query.GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindByIdAsync(request.Id, cancellationToken);

        if (category == null || category.IsDeleted)
        {
            return Result.Failure<Response.GetAllCategories>(new Error("400", "Category not found"));
        }

        var result = new Response.GetAllCategories(category.Id, category.Name, category.Description ?? "",
            category.IsParent, category.ParentId, category.IsDeleted);

        return Result.Success(result);
    }
}