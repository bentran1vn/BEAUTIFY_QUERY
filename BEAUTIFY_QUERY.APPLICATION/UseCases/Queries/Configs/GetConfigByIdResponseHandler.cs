using BEAUTIFY_QUERY.CONTRACT.Services.Configs;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Configs;
internal sealed class GetConfigByIdResponseHandler(IRepositoryBase<Config, Guid> _config)
    : IQueryHandler<Query.GetConfigById, Response.GetConfigByIdResponse>
{
    public async Task<Result<Response.GetConfigByIdResponse>> Handle(Query.GetConfigById request,
        CancellationToken cancellationToken)
    {
        var config = await _config.FindByIdAsync(request.Id, cancellationToken);
        if (config == null)
        {
            return Result.Failure<Response.GetConfigByIdResponse>(new Error("404", "Config not found"));
        }

        var response = new Response.GetConfigByIdResponse(config.Id, config.Key, config.Value);
        return Result.Success(response);
    }
}