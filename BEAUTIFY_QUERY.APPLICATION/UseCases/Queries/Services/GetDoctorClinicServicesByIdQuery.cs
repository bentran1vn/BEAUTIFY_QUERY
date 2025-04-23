using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetDoctorClinicServicesByIdQuery(
    IRepositoryBase<DoctorService, Guid> doctorServiceRepository,
    IRepositoryBase<Service, Guid> serviceRepository)
    : IQueryHandler<Query.GetDoctorClinicServicesByIdQuery, PagedResult<Response.GetAllDoctorServiceByIdResponse>>
{
    
    public async Task<Result<PagedResult<Response.GetAllDoctorServiceByIdResponse>>> Handle(Query.GetDoctorClinicServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var isServiceExisted = await serviceRepository.FindByIdAsync(request.ServiceId, cancellationToken);
        
        if (isServiceExisted == null)
            throw new Exception($"Service {request.ServiceId} not found");

        var query = doctorServiceRepository.FindAll(p =>
                p.ServiceId.Equals(request.ServiceId) && !p.IsDeleted)
            .Include(x => x.Doctor)
            .ThenInclude(x => x.DoctorCertificates)
            .Include(x => x.Doctor)
            .ThenInclude(x => x.UserClinics)
            .ThenInclude(x => x.Clinic);

        var doctors = await PagedResult<DoctorService>.CreateAsync(query, request.PageNumber, request.PageSize);

        PagedResult<Response.GetAllDoctorServiceByIdResponse> result = PagedResult<Response.GetAllDoctorServiceByIdResponse>.Create(
            doctors.Items.Select(x =>
            {
                var clinic =  x.Doctor.UserClinics.Where(x => x.ClinicId == x.Clinic.Id).SingleOrDefault();
                return new Response.GetAllDoctorServiceByIdResponse(
                    x.Id,
                    new Response.UserEntity(
                        x.Doctor.Id, x.Doctor.FullName, x.Doctor.Email, x.Doctor.PhoneNumber,
                        x.Doctor.ProfilePicture, x.Doctor.DoctorCertificates.Select(x =>
                            new Response.CertificateEntity()
                            {
                                Id = x.Id,
                                CertificateUrl = x.CertificateUrl,
                                CertificateName = x.CertificateName,
                                ExpiryDate = x.ExpiryDate,
                                Note = x.Note
                            }).ToList()
                    ),
                    new Response.Clinic(
                        clinic.Clinic.Id, clinic.Clinic.Name, clinic.Clinic.Email, clinic.Clinic.Address,
                        clinic.Clinic.PhoneNumber, (TimeSpan)clinic.Clinic.WorkingTimeStart,
                        (TimeSpan)clinic.Clinic.WorkingTimeEnd,
                        clinic.Clinic.ProfilePictureUrl, clinic.Clinic.IsParent, clinic.Clinic.IsActivated,
                        clinic.Clinic.ParentId
                    )
                );
            }).ToList(),
            doctors.PageIndex,
            doctors.PageSize,
            doctors.TotalCount
        );

        return Result.Success(result);
    }
}