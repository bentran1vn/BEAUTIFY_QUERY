using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

public class Query
{
    public record GetAllApplyRequestQuery(int PageIndex, int PageSize) : IQuery<PagedResult<Response.GetApplyRequest>>;
    public record GetDetailApplyRequestQuery(Guid ApplyRequestId) : IQuery<Response.GetApplyRequestById>;
}