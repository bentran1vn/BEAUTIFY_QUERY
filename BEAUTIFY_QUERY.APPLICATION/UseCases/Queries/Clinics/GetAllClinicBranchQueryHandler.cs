using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetAllClinicBranchQueryHandler(IRepositoryBase<Clinic, Guid> _clinicRepository)
    : IQueryHandler<Query.GetAllClinicBranchQuery, List<Response.GetClinicDetail>>
{
    public async Task<Result<List<Response.GetClinicDetail>>> Handle(Query.GetAllClinicBranchQuery request,
        CancellationToken cancellationToken)
    {
        var clinic = await _clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);
        if (clinic == null) return Result.Failure<List<Response.GetClinicDetail>>(new Error("404", "Clinic not found"));
        var clinicBranches =
            await _clinicRepository.FindAll(x => x.ParentId == clinic.Id).ToListAsync(cancellationToken);
        var result = clinicBranches.Select(x =>
            new Response.GetClinicDetail
            (x.Id,
                x.Name, x.Email, x.PhoneNumber, x.Address, x.TaxCode, x.BusinessLicenseUrl, x.OperatingLicenseUrl,
                x.OperatingLicenseExpiryDate, x.ProfilePictureUrl, clinic.TotalBranches.Value, x.IsActivated)).ToList();

        return Result.Success(result);
    }
}