using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Categories;

public class GetAllCategoriesPagingQueryHandler: IQueryHandler<Query.GetAllCategoriesPagingQuery,
    PagedResult<Response.GetAllCategories>>
{
    private readonly IRepositoryBase<Category, Guid> _categoryRepository;

    public GetAllCategoriesPagingQueryHandler(IRepositoryBase<Category, Guid> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedResult<Response.GetAllCategories>>> Handle(Query.GetAllCategoriesPagingQuery request, CancellationToken cancellationToken)
    {
        
        var query = _categoryRepository.FindAll(x => x.IsDeleted == false);
        
        var categories = await PagedResult<Category>.CreateAsync(
            query,
            request.PageIndex,
            request.PageSize
        );

        var list = categories.Items.Select(x => 
            new Response.GetAllCategories(x.Id, x.Name, x.Description ?? "", x.IsParent, x.ParentId, x.IsDeleted)).ToList();
        
        var result = new PagedResult<Response.GetAllCategories>(list, categories.PageIndex, categories.PageSize, categories.TotalCount);

        return Result.Success(result);
    }
}