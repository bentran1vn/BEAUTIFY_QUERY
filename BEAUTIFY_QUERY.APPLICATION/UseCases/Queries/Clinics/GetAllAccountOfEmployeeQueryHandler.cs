using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetAllAccountOfEmployeeQueryHandler(
    IRepositoryBase<Staff, Guid> staffRepository,
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
        var isExist = await  clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.Children);
        if (isExist == null)
            return Result.Failure<PagedResult<Response.GetAccountOfEmployee>>(new Error("404", "Clinic Not Found"));
        
        var query = userClinicRepository.FindAll(x => x.IsDeleted == false);
        
        query = query
            .Include(x => x.Clinic)
            .Include(x => x.User);

        if (isExist.IsParent == true)
        {
            var clinicFinds = await clinicRepository
                .FindAll(x => x.ParentId == request.ClinicId || x.Id == request.ClinicId)
                .ToListAsync(cancellationToken);
            query = query.Where(x => clinicFinds.Select(y => y.Id).Contains(x.ClinicId));
        }
        else
        {
            query = query.Where(x => x.ClinicId == request.ClinicId);
        }
        
        var roles = await roleRepository.FindAll(x => x.IsDeleted == false).ToListAsync(cancellationToken);
        
        if (request.Role != null)
        {
            var roleName = request.Role.ToString() == "DOCTOR" ? "DOCTOR" : "CLINIC STAFF";
            var role = roles.FirstOrDefault(x => x.Name == roleName);
            query = query.Where(x => x.User.RoleId == role.Id);
        }
        else
        {
            var rolesRequire = roles.Where(x => x.Name == "DOCTOR" || x.Name == "CLINIC STAFF").ToList();
            query = query.Where(x => rolesRequire.Select(r => r.Id).ToList().Contains((Guid)x.User.RoleId));
        }
        
        if(request.SearchTerm != null)
        {
            query = query.Where(x =>
                x.User.FirstName.Contains(request.SearchTerm) ||
                x.User.LastName.Contains(request.SearchTerm) ||
                x.User.Email.Contains(request.SearchTerm)||
                x.Clinic.Address.Contains(request.SearchTerm));
        }
        
        var pagedResult = await PagedResult<UserClinic>.CreateAsync(query, request.PageIndex, request.PageSize);

        var users = pagedResult.Items
            .Select(u => new Response.GetAccountOfEmployee
            {
                Id = u.Id,
                ClinicId = u.ClinicId,
                FirstName = u.User.FirstName,
                LastName = u.User.LastName,
                Email = u.User.Email,
                FullName = u.User.FirstName + " " + u.User.LastName,
                PhoneNumber = u.User.PhoneNumber,
                City = u.User.City,
                District = u.User.District,
                Ward = u.User.Ward,
                FullAddress = u.User.FullAddress,
                Address = u.User.Address,
                ProfilePictureUrl = u.User.ProfilePicture,
                Role = u.User.Role.Name,
            }).ToList();
        
        var result = new PagedResult<Response.GetAccountOfEmployee>(users, pagedResult.TotalCount, pagedResult.PageIndex, pagedResult.PageSize);

        return Result.Success(result);
    }
}