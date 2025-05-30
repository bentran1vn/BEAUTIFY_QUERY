﻿using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.DoctorCertificates;
internal sealed class GetDoctorCertificateByIdQueryHandler(
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetDoctorCertificateById, Response.GetDoctorCertificateByResponse>
{
    public async Task<Result<Response.GetDoctorCertificateByResponse>> Handle(
        Query.GetDoctorCertificateById request, CancellationToken cancellationToken)
    {
        var doctorCertificate = await doctorCertificateRepository
            .FindAll(x => x.Id == request.Id)
            .Select(x => new Response.GetDoctorCertificateByResponse
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                CategoryName = x.Category.Name,
                CategoryId = x.CategoryId,
                CertificateUrl = x.CertificateUrl,
                DoctorName = x.Doctor.FullName,
                ExpiryDate = x.ExpiryDate,
                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (doctorCertificate is null)
            return Result.Failure<Response.GetDoctorCertificateByResponse>(
                new Error("404", $"Doctor certificate not found with ID: {request.Id}"));

        return Result.Success(doctorCertificate);
    }
}