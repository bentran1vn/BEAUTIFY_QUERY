namespace BEAUTIFY_QUERY.CONTRACT.Services.SurveyQuestions;
public static class Query
{
    public record GetSurveyQuestionBySurveyId(Guid SurveyId, int PageIndex = 1, int PageSize = 10)
        : IQuery<PagedResult<Response.SurveyQuestionResponse>>;
}