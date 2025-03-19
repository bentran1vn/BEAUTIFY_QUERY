using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetAllAccountOfEmployeeQueryHandler(
    IRepositoryBase<Staff, Guid> staffRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<Role, Guid> roleRepository)
    : IQueryHandler<Query.GetAllAccountOfEmployeeQuery,
        List<Response.GetAccountOfEmployee>>
{
    public async Task<Result<List<Response.GetAccountOfEmployee>>> Handle(
        Query.GetAllAccountOfEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var roleName = request.role.ToString() == "DOCTOR" ? "DOCTOR" : "CLINIC STAFF";
        var role = await roleRepository.FindSingleAsync(x => x.Name == roleName, cancellationToken);
        if (role == null)
            return Result.Failure<List<Response.GetAccountOfEmployee>>(new Error("404", "Role Not Found"));

        var clinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.UserClinics);

        if (clinic == null)
            return Result.Failure<List<Response.GetAccountOfEmployee>>(new Error("404", "Clinic Not Found"));

        var userIds = clinic.UserClinics?.Where(x => x.User!.RoleId == role.Id).Select(x => x.UserId).ToList();
        if (userIds == null || userIds.Count == 0)
            return Result.Failure<List<Response.GetAccountOfEmployee>>(new Error("404",
                "No users found for the specified role"));

        var users = await staffRepository.FindAll(x => userIds.Contains(x.Id))
            .Select(u => new Response.GetAccountOfEmployee
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                FullName = u.FirstName + " " + u.LastName,
                PhoneNumber = u.PhoneNumber,
                City = u.City,
                District = u.District,
                Ward = u.Ward,
                FullAddress = u.FullAddress,
                Address = u.Address,
                ProfilePictureUrl = u.ProfilePicture,
                Role = u.Role.Name
            }).ToListAsync(cancellationToken);

        return Result.Success(users);
    }
}