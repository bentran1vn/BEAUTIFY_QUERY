using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using BEAUTIFY_QUERY.DOMAIN.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;

internal sealed class GetDoctorCertificateById : IQueryHandler<Query.GetDoctorCertificateByDoctorId, 
    IReadOnlyList<Response.GetDoctorCertificateByResponse>>
{
    private readonly IRepositoryBase<User, Guid> _userRepository;
    private readonly IRepositoryBase<DoctorCertificate, Guid> _doctorCertificateRepository;

    public GetDoctorCertificateById(
        IRepositoryBase<User, Guid> userRepository, 
        IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    {
        _userRepository = userRepository;
        _doctorCertificateRepository = doctorCertificateRepository;
    }

    public async Task<Result<IReadOnlyList<Response.GetDoctorCertificateByResponse>>> Handle(
        Query.GetDoctorCertificateByDoctorId request, 
        CancellationToken cancellationToken)
    {
        var userExists = await _userRepository.FindAll(u => u.Id == request.DoctorId)
            .AnyAsync(cancellationToken);

        if (!userExists)
        {
            throw new DoctorCertificateException.DoctorCertificateNotFoundException(request.DoctorId);
        }

        var certificates = await _doctorCertificateRepository.FindAll(x => x.DoctorId == request.DoctorId)
            .Select(x => new Response.GetDoctorCertificateByResponse
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                CertificateUrl = x.CertificateUrl,
                DoctorName = $"{x.Doctor.FirstName} {x.Doctor.LastName}",
                ExpiryDate = x.ExpiryDate,
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<Response.GetDoctorCertificateByResponse>>(certificates);
    }
}
