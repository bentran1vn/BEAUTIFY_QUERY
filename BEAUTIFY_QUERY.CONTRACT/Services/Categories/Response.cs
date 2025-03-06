namespace BEAUTIFY_QUERY.CONTRACT.Services.Categories;
public class Response
{
    public record GetAllCategories(
        Guid Id,
        string Name,
        string Description,
        bool IsParent,
        Guid? ParentId,
        bool IsDeleted
    );

    public record GetAllCategoriesWithSubCategories(
        Guid Id,
        string Name,
        string Description,
        bool IsParent,
        Guid? ParentId,
        bool IsDeleted,
        List<GetAllCategories> SubCategories
    );
}