using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetDoctorClinicServicesByIdQueryV2Handler(
    IMongoRepository<ClinicServiceProjection> clinicServiceRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetDoctorClinicServicesByIdQueryV2, Response.GetAllDoctorServiceByIdResponseV2>
{
    public async Task<Result<Response.GetAllDoctorServiceByIdResponseV2>> Handle(Query.GetDoctorClinicServicesByIdQueryV2 request, CancellationToken cancellationToken)
    {
        var isServiceExisted = await clinicServiceRepository.FindOneAsync(p => p.DocumentId.Equals(request.ServiceId));
        
        var listDoctorId = isServiceExisted.DoctorServices.Select(x => x.Doctor.Id).ToList();
        var doctorCertificates = await doctorCertificateRepository
            .FindAll(x => listDoctorId.Contains(x.DoctorId))
            .ToListAsync(cancellationToken);
        
        Response.GetAllDoctorServiceByIdResponseV2 result;

        result = new Response.GetAllDoctorServiceByIdResponseV2(
            isServiceExisted.DocumentId,
            isServiceExisted.DoctorServices.Select(y => new Response.DoctorService(
                y.Id,
                y.ClinicId,
                y.ServiceId,
                y.Rating,
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
                        }).ToList()))).ToList());
        
        return Result.Success(result);
    }
}