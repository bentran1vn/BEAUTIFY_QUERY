using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Surveys;
public static class Query
{
    public record GetSurvey(
        string? searchTerm,
        string? SortColumn,
        SortOrder SortOrder,
        int PageNumber,
        int PageSize) : IQuery<PagedResult<Response.SurveyResponse>>;
}