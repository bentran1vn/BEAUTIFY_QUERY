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

        // Handle complex search terms with both IsParent and name conditions
        if (searchTerm.Contains("and", StringComparison.OrdinalIgnoreCase))
        {
            var parts = searchTerm.Split(["and"],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var isParentCondition =
                parts.FirstOrDefault(p => p.Contains("IsParent==", StringComparison.OrdinalIgnoreCase));
            var nameCondition = parts.FirstOrDefault(p => p.Contains("name=", StringComparison.OrdinalIgnoreCase));

            var query = categoryRepository.FindAll(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(isParentCondition))
            {
                var isParentValue = isParentCondition.Split('=').Last().Trim().Trim('"');
                if (bool.TryParse(isParentValue, out var isParent)) query = query.Where(x => x.IsParent == isParent);
            }

            if (!string.IsNullOrEmpty(nameCondition))
            {
                var nameValue = nameCondition.Split('=').Last().Trim().Trim('"');
                if (!string.IsNullOrEmpty(nameValue)) query = query.Where(x => x.Name.Contains(nameValue));
            }

            var categories = await PagedResult<Category>.CreateAsync(
                query,
                request.PageIndex,
                request.PageSize
            );

            var list = categories.Items.Select(x =>
                    new Response.GetAllCategories(x.Id, x.Name, x.Description ?? "", x.IsParent, x.ParentId,
                        x.IsDeleted))
                .ToList();

            var result = new PagedResult<Response.GetAllCategories>(list, categories.PageIndex, categories.PageSize,
                categories.TotalCount);

            return Result.Success(result);
        }

        // Original simple search term handling
        searchTerm = searchTerm switch
        {
            "IsParent==true" => "true",
            "IsParent==false" => "false",
            _ => searchTerm
        };

        var simpleQuery = categoryRepository.FindAll(x => !x.IsDeleted);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (searchTerm.Equals("true", StringComparison.OrdinalIgnoreCase))
                simpleQuery = simpleQuery.Where(x => x.IsParent);
            else if (searchTerm.Equals("false", StringComparison.OrdinalIgnoreCase))
                simpleQuery = simpleQuery.Where(x => !x.IsParent);
            else
                simpleQuery = simpleQuery.Where(x => x.Name.Contains(searchTerm));
        }

        var simpleCategories = await PagedResult<Category>.CreateAsync(
            simpleQuery,
            request.PageIndex,
            request.PageSize
        );

        var simpleList = simpleCategories.Items.Select(x =>
                new Response.GetAllCategories(x.Id, x.Name, x.Description ?? "", x.IsParent, x.ParentId, x.IsDeleted))
            .ToList();

        var simpleResult = new PagedResult<Response.GetAllCategories>(simpleList, simpleCategories.PageIndex,
            simpleCategories.PageSize,
            simpleCategories.TotalCount);

        return Result.Success(simpleResult);
    }
}