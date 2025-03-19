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


    public record GetAllAccountOfEmployeeQuery(Guid ClinicId, Roles? Role, string? SearchTerm,
        int PageIndex, int PageSize)
        : IQuery<PagedResult<Response.GetAccountOfEmployee>>;
    
    public record GetDetailAccountOfEmployeeQuery(Guid ClinicId, Guid StaffId)
        : IQuery<Response.GetAccountOfEmployee>;

    public enum Roles
    {
        DOCTOR = 1,
        CLINIC_STAFF = 2
    }

    public record GetAllClinicBranchQuery(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetClinicBranches>>;
}