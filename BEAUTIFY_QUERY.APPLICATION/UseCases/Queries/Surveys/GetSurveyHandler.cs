using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Surveys;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Surveys;
internal sealed class GetSurveyHandler(IRepositoryBase<Survey, Guid> surveyRepositoryBase)
    : IQueryHandler<Query.GetSurvey, PagedResult<Response.SurveyResponse>>
{
    public async Task<Result<PagedResult<Response.SurveyResponse>>> Handle(Query.GetSurvey request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;

        // Query directly with projection to reduce memory usage and avoid over-fetching
        var query = surveyRepositoryBase
            .FindAll(x => !x.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.Description.Contains(searchTerm) ||
                x.Id.ToString().Contains(searchTerm));

        query = query
            .Include(x => x.SurveyQuestions)! // Include only if needed
            .ThenInclude(q => q.SurveyQuestionOptions);

        // Simplified sorting using inline switch
        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request.SortColumn))
            : query.OrderBy(GetSortProperty(request.SortColumn));

        var surveys = await PagedResult<Survey>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );

        var result = new PagedResult<Response.SurveyResponse>(
            surveys.Items.Select(survey => new Response.SurveyResponse
            {
                Id = survey.Id,
                Name = survey.Name,
                Description = survey.Description,
                CategoryName = survey.Category?.Name,
                Questions = survey.SurveyQuestions.Select(q => new Response.SurveyQuestionResponse
                {
                    Id = q.Id,
                    Question = q.Question,
                    QuestionType = q.QuestionType.ToString(),
                    Options = q.SurveyQuestionOptions
                        .SelectMany(opt => opt.Option
                            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(option => new Response.SurveyQuestionOptionResponse
                            {
                                Option = option.Trim()
                            }))
                        .ToList()
                }).ToList()
            }).ToList(),
            surveys.PageIndex,
            surveys.PageSize,
            surveys.TotalCount
        );

        return Result.Success(result);
    }

    private static Expression<Func<Survey, object>> GetSortProperty(string? sortColumn)
    {
        return sortColumn?.ToLower() switch
        {
            "name" => x => x.Name,
            "description" => x => x.Description,
            _ => x => x.CreatedOnUtc
        };
    }
}