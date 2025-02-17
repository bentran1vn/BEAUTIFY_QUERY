using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Categories;

public class GetAllCategoriesQueryHandler: IQueryHandler<Query.GetAllCategoriesQuery,
    List<Response.GetAllCategories>>
{
    private readonly IRepositoryBase<Category, Guid> _categoryRepository;

    public GetAllCategoriesQueryHandler(IRepositoryBase<Category, Guid> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<Response.GetAllCategories>>> Handle(Query.GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.FindAll().ToListAsync(cancellationToken);

        var result = categories.Select(x => 
            new Response.GetAllCategories(x.Id, x.Name, x.Description ?? "", x.IsParent, x.ParentId, x.IsDeleted)).ToList();

        return Result.Success(result);
    }
}