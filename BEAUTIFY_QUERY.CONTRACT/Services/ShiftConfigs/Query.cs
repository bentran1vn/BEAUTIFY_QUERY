namespace BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;

public static class Query
{
    public record GetShiftConfigQuery(
        Guid ClinicId,
        int PageNumber,
        int PageSize)
        : IQuery<PagedResult<Response.ShiftResponse>>;
}