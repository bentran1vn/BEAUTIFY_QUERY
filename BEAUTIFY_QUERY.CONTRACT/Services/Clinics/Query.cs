using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
public class Query
{
    public record GetClinicsQuery(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetClinics>>;

    public record GetClinicDetailQuery(Guid id) : IQuery<Response.GetClinicDetail>;

    public record GetAllApplyRequestQuery(int PageIndex, int PageSize) : IQuery<PagedResult<Response.GetApplyRequest>>;

    public record GetDetailApplyRequestQuery(Guid ApplyRequestId) : IQuery<Response.GetApplyRequestById>;


    public record GetAllAccountOfEmployeeQuery(Guid ClinicId)
        : IQuery<List<Response.GetAccountOfEmployee>>;

    public record GetAllClinicBranchQuery(string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetClinicBranches>>;
}