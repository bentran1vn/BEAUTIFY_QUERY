using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.SurveyQuestions;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.SurveyQuestions;
internal sealed class GetSurveyQuestionBySurveyIdHandler(
    IRepositoryBase<Survey, Guid> surveyRepositoryBase,
    IRepositoryBase<SurveyQuestion, Guid> surveyQuestionRepositoryBase
) : IQueryHandler<Query.GetSurveyQuestionBySurveyId, PagedResult<Response.SurveyQuestionResponse>>
{
    public async Task<Result<PagedResult<Response.SurveyQuestionResponse>>> Handle(
        Query.GetSurveyQuestionBySurveyId request,
        CancellationToken cancellationToken)
    {
        var survey =
            await surveyRepositoryBase.FindByIdAsync(request.SurveyId, cancellationToken, x => x.SurveyQuestions);
        if (survey == null)
            return Result.Failure<PagedResult<Response.SurveyQuestionResponse>>(new Error("404", "Survey not found"));

        var surveyQuestions = surveyQuestionRepositoryBase.FindAll(x => x.Id != request.SurveyId && !x.IsDeleted);

        var result = await PagedResult<SurveyQuestion>.CreateAsync(
            surveyQuestions,
            request.PageIndex,
            request.PageSize
        );

        var mappedSurveyQuestions = result.Items.Select(sq => new Response.SurveyQuestionResponse
        {
            Id = sq.Id,
            Question = sq.Question,
            QuestionType = sq.QuestionType,
            SurveyId = sq.SurveyId
        }).ToList();

        return Result.Success(new PagedResult<Response.SurveyQuestionResponse>(
            mappedSurveyQuestions,
            result.PageIndex,
            result.PageSize,
            result.TotalCount
        ));
    }
}