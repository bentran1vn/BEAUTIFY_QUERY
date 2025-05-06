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
        // Check if clinic exists
        var clinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.Children);
        if (clinic == null)
            return Result.Failure<PagedResult<Response.GetAccountOfEmployee>>(new Error("404", "Clinic Not Found"));

        // Get applicable clinic IDs
        var clinicIds = clinic.IsParent.Value
            ? clinic.Children.Select(x => x.Id).Append(request.ClinicId).ToList()
            : [request.ClinicId];

        // Fix: Use SQL-compatible comparison for role names
        var roles = await roleRepository
            .FindAll(x => x.IsDeleted == false && (
                x.Name.ToUpper() == "DOCTOR" ||
                x.Name.ToUpper() == "CLINIC STAFF"
            ))
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();

        // Apply role filter if specified
        if (request.Role != null)
        {
            var roleName = request.Role.ToString() == "DOCTOR" ? "DOCTOR" : "CLINIC STAFF";
            roleIds = roles
                .Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id)
                .ToList();
        }

        // Build base query with initial filters for improved performance
        var query = userClinicRepository.FindAll(x =>
            x.IsDeleted == false &&
            clinicIds.Contains(x.ClinicId) &&
            roleIds.Contains((Guid)x.User.RoleId));

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(x =>
                x.User.FirstName.Contains(request.SearchTerm) ||
                x.User.LastName.Contains(request.SearchTerm) ||
                x.User.Email.Contains(request.SearchTerm) ||
                x.Clinic.Address.Contains(request.SearchTerm));
        }

        // Get unique user IDs for pagination
        var totalCount = await query
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Apply pagination early at user level
        var userIds = await query
            .Select(x => x.UserId)
            .Distinct()
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Only get data for users in the current page
        var employeeData = await query
            .Where(x => userIds.Contains(x.UserId))
            .Include(x => x.Clinic)
            .Include(x => x.User)
            .ThenInclude(u => u.DoctorCertificates)
            .ThenInclude(c => c.Category)
            .ToListAsync(cancellationToken);

        // Build response using memory operations on the already filtered dataset
        var result = employeeData
            .GroupBy(x => x.UserId)
            .Select(g => new Response.GetAccountOfEmployee
            {
                Branchs = g.Select(item => new Response.GetClinicBranches(
                    item.Clinic.Id, item.Clinic.Name,
                    item.Clinic.Email, item.Clinic.City,
                    item.Clinic.Address, item.Clinic.District,
                    item.Clinic.Ward, item.Clinic.FullAddress,
                    item.Clinic.TaxCode,
                    item.Clinic.WorkingTimeStart,
                    item.Clinic.WorkingTimeEnd,
                    item.Clinic.BusinessLicenseUrl,
                    item.Clinic.OperatingLicenseUrl, item.Clinic.OperatingLicenseExpiryDate,
                    item.Clinic.ProfilePictureUrl, item.Clinic.IsActivated
                )).ToArray(),
                EmployeeId = g.Key,
                FirstName = g.First().User.FirstName,
                LastName = g.First().User.LastName,
                Email = g.First().User.Email,
                FullName = g.First().User.FullName,
                PhoneNumber = g.First().User.PhoneNumber,
                City = g.First().User.City,
                District = g.First().User.District,
                Ward = g.First().User.Ward,
                FullAddress = g.First().User.FullAddress,
                Address = g.First().User.Address,
                ProfilePictureUrl = g.First().User.ProfilePicture,
                Role = g.First().User.Role.Name,
                DoctorCertificates = g.First().User.DoctorCertificates
                    .Select(x => new Response.DoctorCertificates
                    {
                        Id = x.Id,
                        CertificateUrl = x.CertificateUrl,
                        ExpiryDate = x.ExpiryDate,
                        CategoryId = x.CategoryId,
                        CategoryName = x.Category.Name,
                        Note = x.Note
                    }).ToList()
            })
            .ToList();

        // Create paged result manually since we've already applied pagination
        var pagedResult = new PagedResult<Response.GetAccountOfEmployee>(
            result,
            request.PageIndex,
            request.PageSize, totalCount);

        return Result.Success(pagedResult);
    }
}