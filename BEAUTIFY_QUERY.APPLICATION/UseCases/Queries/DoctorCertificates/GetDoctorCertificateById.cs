using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;

internal sealed class GetDoctorCertificatesByDoctorIdHandler(
    IRepositoryBase<Staff, Guid> staffRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetDoctorCertificateByDoctorId,
        IReadOnlyList<Response.GetDoctorCertificateByResponse>>
{
    public async Task<Result<IReadOnlyList<Response.GetDoctorCertificateByResponse>>> Handle(
        Query.GetDoctorCertificateByDoctorId request,
        CancellationToken cancellationToken)
    {
        var doctorExists = await staffRepository.FindAll(u => u.Id == request.DoctorId)
            .AnyAsync(cancellationToken);

        if (!doctorExists)
            return Result.Failure<IReadOnlyList<Response.GetDoctorCertificateByResponse>>(
                new Error("404", $"Doctor not found with ID: {request.DoctorId}"));

        var certificateResponses = await doctorCertificateRepository
            .FindAll(x => x.DoctorId == request.DoctorId)
            .Include(x => x.Doctor)
            .Select(x => new Response.GetDoctorCertificateByResponse
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                CertificateUrl = x.CertificateUrl,
                DoctorName = $"{x.Doctor.FirstName} {x.Doctor.LastName}",
                ExpiryDate = x.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<Response.GetDoctorCertificateByResponse>>(certificateResponses);
    }
}