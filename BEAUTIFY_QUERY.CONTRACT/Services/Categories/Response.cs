namespace BEAUTIFY_QUERY.CONTRACT.Services.Categories;

public class Response
{
    public record GetAllCategories(Guid Id, string Name, string Description, bool IsParent, Guid? ParentId, bool IsDeleted);
}