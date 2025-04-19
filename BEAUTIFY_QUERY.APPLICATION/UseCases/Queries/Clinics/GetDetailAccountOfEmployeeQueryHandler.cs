using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetDetailAccountOfEmployeeQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<UserClinic, Guid> userClinicRepository) :
    IQueryHandler<Query.GetDetailAccountOfEmployeeQuery,
        Response.GetAccountOfEmployee>
{
    public async Task<Result<Response.GetAccountOfEmployee>> Handle(Query.GetDetailAccountOfEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var isExistClinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.Children);
        if (isExistClinic == null)
            return Result.Failure<Response.GetAccountOfEmployee>(new Error("404", "Clinic Not Found"));

        var query = userClinicRepository
            .FindAll(x => x.IsDeleted == false && x.UserId == request.StaffId);

        query = query.Include(x => x.User)
            .ThenInclude(x => x.DoctorCertificates);

        Response.GetAccountOfEmployee? result = null;

        if (isExistClinic.IsParent == true)
        {
            var childrenIds = isExistClinic.Children.Select(x => x.Id).ToList();
            childrenIds.Add(request.ClinicId);
            query = query.Where(x => childrenIds.Contains(x.ClinicId));

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
                    Role = g.Select(x => x.User.Role.Name).FirstOrDefault(),
                    DoctorCertificates = g.SelectMany(x => x.User.DoctorCertificates)
                        .Select(x => new Response.DoctorCertificates
                        {
                            Id = x.Id,
                            CertificateName = x.CertificateName,
                            CertificateUrl = x.CertificateUrl,
                            ExpiryDate = x.ExpiryDate,
                            Note = x.Note
                        }).ToList()
                });

            var list = await groupByQuery.ToListAsync(cancellationToken);
            result = list.FirstOrDefault();
        }
        else
        {
            query = query.Where(x => x.ClinicId == request.ClinicId);

            var isExist = await query.FirstOrDefaultAsync(cancellationToken);

            if (isExist == null)
                return Result.Failure<Response.GetAccountOfEmployee>(new Error("404", "Staff Not Found"));

            result = new Response.GetAccountOfEmployee
            {
                Branchs =
                [
                    new Response.GetClinicBranches(
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
                    )
                ],
                EmployeeId = isExist.User!.Id,
                FirstName = isExist.User!.FirstName,
                LastName = isExist.User!.LastName,
                Email = isExist.User!.Email,
                FullName = isExist.User?.FirstName + " " + isExist.User?.LastName,
                PhoneNumber = isExist.User?.PhoneNumber,
                City = isExist.User?.City,
                District = isExist.User?.District,
                Ward = isExist.User?.Ward,
                FullAddress = isExist.User?.FullAddress,
                Address = isExist.User?.Address,
                ProfilePictureUrl = isExist.User?.ProfilePicture,
                Role = isExist.User?.Role?.Name,
                DoctorCertificates = isExist.User?.DoctorCertificates?.Select(x => new Response.DoctorCertificates
                {
                    Id = x.Id,
                    CertificateName = x.CertificateName,
                    CertificateUrl = x.CertificateUrl,
                    ExpiryDate = x.ExpiryDate,
                    Note = x.Note
                }).ToList()
            };
        }

        return Result.Success(result);
    }
}