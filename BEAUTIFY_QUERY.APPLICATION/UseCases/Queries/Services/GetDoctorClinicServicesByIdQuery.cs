using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetDoctorClinicServicesByIdQuery(
    IMongoRepository<ClinicServiceProjection> clinicServiceRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetDoctorClinicServicesByIdQuery, Response.GetAllDoctorServiceByIdResponse>
{
    
    public async Task<Result<Response.GetAllDoctorServiceByIdResponse>> Handle(Query.GetDoctorClinicServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var isServiceExisted = await clinicServiceRepository.FindOneAsync(p => p.DocumentId.Equals(request.ServiceId));

        if (isServiceExisted == null)
            throw new Exception($"Service {request.ServiceId} not found");
        
        var listDoctorId = isServiceExisted.DoctorServices.Select(x => x.Doctor.Id).ToList();
        var doctorCertificates = await doctorCertificateRepository
            .FindAll(x => listDoctorId.Contains(x.DoctorId))
            .ToListAsync(cancellationToken);

        Response.GetAllDoctorServiceByIdResponse result;

        result = new Response.GetAllDoctorServiceByIdResponse(
            isServiceExisted.DocumentId,
            isServiceExisted.DoctorServices.Select(y => new Response.DoctorService(
                y.Id,
                y.ServiceId,
                new Response.UserEntity(
                    y.Doctor.Id,
                    y.Doctor.FullName,
                    y.Doctor.Email,
                    y.Doctor.PhoneNumber,
                    y.Doctor.ProfilePictureUrl,
                    doctorCertificates
                        .Where(c => c.DoctorId == y.Doctor.Id)
                        .Select(c => new Response.CertificateEntity
                        {
                            Id = c.Id,
                            CertificateUrl = c.CertificateUrl,
                            CertificateName = c.CertificateName,
                            ExpiryDate = c.ExpiryDate,
                            Note = c.Note
                        }).ToList()))).ToList()
        );

        return Result.Success(result);
    }
}