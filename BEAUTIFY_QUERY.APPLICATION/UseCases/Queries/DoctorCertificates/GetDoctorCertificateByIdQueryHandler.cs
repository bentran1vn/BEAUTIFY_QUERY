using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;
public class
    GetDoctorCertificateByIdQueryHandler : IQueryHandler<Query.GetDoctorCertificateById,
    Response.GetDoctorCertificateByResponse>
{
    private readonly IRepositoryBase<DoctorCertificate, Guid> _doctorCertificateRepository;

    public GetDoctorCertificateByIdQueryHandler(IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    {
        _doctorCertificateRepository = doctorCertificateRepository;
    }
    public async Task<Result<Response.GetDoctorCertificateByResponse>> Handle(Query.GetDoctorCertificateById request,
        CancellationToken cancellationToken)
    {
        var doctorCertificate = _doctorCertificateRepository.FindAll(x => x.Id == request.Id)
            .Select(x => new Response.GetDoctorCertificateByResponse
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                CertificateUrl = x.CertificateUrl,
                DoctorName = $"{x.Doctor.FirstName} {x.Doctor.LastName}",
                ExpiryDate = x.ExpiryDate,
                Note = x.Note
            }).FirstOrDefault();
        return Result.Success(doctorCertificate);
    }
}