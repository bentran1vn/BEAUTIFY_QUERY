namespace BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;

public static class Query
{
    public record GetShiftConfigQuery(
        Guid ClinicId,
        string RoleName,
        int PageNumber,
        int PageSize)
        : IQuery<PagedResult<Response.ShiftResponse>>;
}