using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetAllAccountOfEmployeeQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<UserClinic, Guid> userClinicRepository,
    IRepositoryBase<Role, Guid> roleRepository)
    : IQueryHandler<Query.GetAllAccountOfEmployeeQuery,
        PagedResult<Response.GetAccountOfEmployee>>
{
    public async Task<Result<PagedResult<Response.GetAccountOfEmployee>>> Handle(
        Query.GetAllAccountOfEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var isExist = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.Children);
        if (isExist == null)
            return Result.Failure<PagedResult<Response.GetAccountOfEmployee>>(new Error("404", "Clinic Not Found"));

        var query = userClinicRepository.FindAll(x => x.IsDeleted == false);

        query = query
            .Include(x => x.Clinic)
            .Include(x => x.User);

        if (isExist.IsParent == true)
        {
            var childrenIds = isExist.Children.Select(x => x.Id).ToList();
            childrenIds.Add(request.ClinicId);
            query = query.Where(x => childrenIds.Contains(x.ClinicId));
        }
        else
        {
            query = query.Where(x => x.ClinicId == request.ClinicId);
        }

        var roles = await roleRepository.FindAll(x => x.IsDeleted == false).ToListAsync(cancellationToken);

        if (request.Role != null)
        {
            var roleName = request.Role.ToString() == "DOCTOR" ? "DOCTOR" : "CLINIC STAFF";
            var role = roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
            query = query.Where(x => x.User.RoleId == role.Id);
        }
        else
        {
            var rolesRequire = roles.Where(x =>
                x.Name.Equals("DOCTOR", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.Equals("CLINIC STAFF", StringComparison.CurrentCultureIgnoreCase)).ToList();
            query = query.Where(x => rolesRequire.Select(r => r.Id).ToList().Contains((Guid)x.User.RoleId));
        }

        if (request.SearchTerm != null)
            query = query.Where(x =>
                x.User.FirstName.Contains(request.SearchTerm) ||
                x.User.LastName.Contains(request.SearchTerm) ||
                x.User.Email.Contains(request.SearchTerm) ||
                x.Clinic.Address.Contains(request.SearchTerm));

        var groupByQuery = query
            .GroupBy(x => x.UserId)
            .Select(g => new Response.GetAccountOfEmployee
            {
                Branchs = g.Select(isExist => new Response.GetClinicBranches(
                    isExist.Clinic.Id, isExist.Clinic.Name,
                    isExist.Clinic.Email, isExist.Clinic.City,
                    isExist.Clinic.Address, isExist.Clinic.District,
                    isExist.Clinic.Ward, isExist.Clinic.FullAddress,
                    isExist.Clinic.TaxCode,
                    isExist.Clinic.WorkingTimeStart,
                    isExist.Clinic.WorkingTimeEnd,
                    isExist.Clinic.BusinessLicenseUrl,
                    isExist.Clinic.OperatingLicenseUrl, isExist.Clinic.OperatingLicenseExpiryDate,
                    isExist.Clinic.ProfilePictureUrl, isExist.Clinic.IsActivated
                )).ToArray(),
                EmployeeId = g.Key,
                FirstName = g.Select(x => x.User.FirstName).FirstOrDefault(),
                LastName = g.Select(x => x.User.LastName).FirstOrDefault(),
                Email = g.Select(x => x.User.Email).FirstOrDefault(),
                FullName = g.Select(x => x.User.FirstName + " " + x.User.LastName).FirstOrDefault(),
                PhoneNumber = g.Select(x => x.User.PhoneNumber).FirstOrDefault(),
                City = g.Select(x => x.User.City).FirstOrDefault(),
                District = g.Select(x => x.User.District).FirstOrDefault(),
                Ward = g.Select(x => x.User.Ward).FirstOrDefault(),
                FullAddress = g.Select(x => x.User.FullAddress).FirstOrDefault(),
                Address = g.Select(x => x.User.Address).FirstOrDefault(),
                ProfilePictureUrl = g.Select(x => x.User.ProfilePicture).FirstOrDefault(),
                Role = g.Select(x => x.User.Role.Name).FirstOrDefault()
            });

        var result =
            await PagedResult<Response.GetAccountOfEmployee>.CreateAsync(groupByQuery, request.PageIndex,
                request.PageSize);

        return Result.Success(result);
    }
}