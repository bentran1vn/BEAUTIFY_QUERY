namespace BEAUTIFY_QUERY.CONTRACT.Services.Configs;
public static class Response
{
    public record GetConfigsResponse(
        Guid Id,
        string Key,
        string Value
    );

    public record GetConfigByIdResponse(
        Guid Id,
        string Key,
        string Value
    );
}