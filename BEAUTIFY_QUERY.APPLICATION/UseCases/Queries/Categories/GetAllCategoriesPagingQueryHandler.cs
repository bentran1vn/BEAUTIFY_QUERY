using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Categories;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Categories;
public class GetAllCategoriesPagingQueryHandler(IRepositoryBase<Category, Guid> categoryRepository)
    : IQueryHandler<Query.GetAllCategoriesPagingQuery,
        PagedResult<Response.GetAllCategories>>
{
    public async Task<Result<PagedResult<Response.GetAllCategories>>> Handle(Query.GetAllCategoriesPagingQuery request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        searchTerm = searchTerm switch
        {
            "IsParent==true" => "true",
            "IsParent==false" => "false",
            _ => searchTerm
        };

        var query = categoryRepository.FindAll(x => !x.IsDeleted);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (searchTerm.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.IsParent);
            }
            else if (searchTerm.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => !x.IsParent);
            }
            else
            {
                query = query.Where(x => x.Name.Contains(searchTerm));
            }
        }

        var categories = await PagedResult<Category>.CreateAsync(
            query,
            request.PageIndex,
            request.PageSize
        );

        var list = categories.Items.Select(x =>
                new Response.GetAllCategories(x.Id, x.Name, x.Description ?? "", x.IsParent, x.ParentId, x.IsDeleted))
            .ToList();

        var result = new PagedResult<Response.GetAllCategories>(list, categories.PageIndex, categories.PageSize,
            categories.TotalCount);

        return Result.Success(result);
    }
}