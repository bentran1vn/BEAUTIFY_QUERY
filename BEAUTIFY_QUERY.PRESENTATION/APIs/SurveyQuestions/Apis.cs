using BEAUTIFY_QUERY.CONTRACT.Services.SurveyQuestions;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.SurveyQuestions;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/survey-questions";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("SurveyQuestions").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetSurveyQuestionBySurveyId);
    }

    private static async Task<IResult> GetSurveyQuestionBySurveyId(ISender sender, Guid surveyId, int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetSurveyQuestionBySurveyId(surveyId, pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}