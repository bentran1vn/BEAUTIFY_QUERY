using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Surveys;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Surveys;
internal sealed class GetSurveyHandler(IRepositoryBase<Survey, Guid> surveyRepositoryBase)
    : IQueryHandler<Query.GetSurvey, PagedResult<Response.SurveyResponse>>
{
    public async Task<Result<PagedResult<Response.SurveyResponse>>> Handle(Query.GetSurvey request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        var query = surveyRepositoryBase.FindAll(x=>!x.IsDeleted);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.Description.Contains(searchTerm));
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));
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
            }).ToList(),
            surveys.PageIndex,
            surveys.PageSize,
            surveys.TotalCount
        );
        return Result.Success(result);
    }


    private static Expression<Func<Survey, object>> GetSortProperty(Query.GetSurvey request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            _ => projection => projection.CreatedOnUtc
        };
    }
}