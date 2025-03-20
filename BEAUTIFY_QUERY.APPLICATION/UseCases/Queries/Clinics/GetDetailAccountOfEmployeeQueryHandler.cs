using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetDetailAccountOfEmployeeQueryHandler(
        IRepositoryBase<UserClinic, Guid> userClinicRepository):
    IQueryHandler<Query.GetDetailAccountOfEmployeeQuery,
        Response.GetAccountOfEmployee>
{
    public async Task<Result<Response.GetAccountOfEmployee>> Handle(Query.GetDetailAccountOfEmployeeQuery request, CancellationToken cancellationToken)
    {
        var query =  userClinicRepository
            .FindAll(x => x.ClinicId == request.ClinicId &&
                                  x.UserId == request.StaffId);
        query = query.Include(x => x.User)
            .ThenInclude(x => x.DoctorCertificates);

        var isExist = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (isExist == null)
            return Result.Failure<Response.GetAccountOfEmployee>(new Error("404", "Staff Not Found"));
        
        var result = new Response.GetAccountOfEmployee
        {
            Id = isExist.Id,
            ClinicId = isExist.ClinicId,
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
            DoctorCertificates = isExist.User?.DoctorCertificates?.Select(x => new Response.DoctorCertificates()
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                CertificateUrl = x.CertificateUrl,
                ExpiryDate = x.ExpiryDate,
                Note = x.Note
            }).ToList()
        };

        return Result.Success(result);
    }
}