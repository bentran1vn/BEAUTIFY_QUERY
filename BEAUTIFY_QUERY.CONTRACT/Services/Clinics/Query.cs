using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
public static class Query
{
    public enum Roles
    {
        DOCTOR = 1,
        CLINIC_STAFF = 2
    }

    public record GetClinicsQuery(
        string? SearchTerm,
        string? Role,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetClinics>>;

    public record GetClinicDetailQuery(
        Guid id,
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<Response.GetClinicDetail>;
    
    public record GetClinicMyQuery(
        Guid ClinicId,
        string RoleName) : IQuery<Response.MyClinicApply>;

    public record GetAllApplyRequestQuery(int PageIndex, int PageSize) : IQuery<PagedResult<Response.GetApplyRequest>>;
    
    public record GetAllApplyBranchRequestQuery(Guid? ClinicId, string? SearchTerm, int PageIndex, int PageSize) : IQuery<PagedResult<Response.BranchClinicApplyGetAll>>;

    public record GetDetailApplyRequestQuery(Guid ApplyRequestId) : IQuery<Response.GetApplyRequestById>;
    
    public record GetDetailBranchApplyRequestQuery(Guid ApplyRequestId) : IQuery<Response.BranchClinicApplyDetail>;
    
    public record GetAllAccountOfEmployeeQuery(
        Guid ClinicId,
        Roles? Role,
        string? SearchTerm,
        int PageIndex,
        int PageSize)
        : IQuery<PagedResult<Response.GetAccountOfEmployee>>;

    public record GetDetailAccountOfEmployeeQuery(Guid ClinicId, Guid StaffId)
        : IQuery<Response.GetAccountOfEmployee>;

    public record GetAllClinicBranchQuery(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.GetClinicBranches>>;

    public record GetClinicBranchesQuery(Guid? Id, string Role) : IQuery<Response.GetClinicBranchesResponse>;

    public record GetClinicByIdQuery(Guid Id) : IQuery<Response.ClinicBranchDto>;

    public record GetSubClinicByIdQuery(Guid Id) : IQuery<Response.ClinicBranchDto>;
}