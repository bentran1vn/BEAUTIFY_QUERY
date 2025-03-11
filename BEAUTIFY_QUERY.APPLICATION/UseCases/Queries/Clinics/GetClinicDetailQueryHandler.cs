using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetClinicDetailQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetClinicDetailQuery, Response.GetClinicDetail>
{
    public async Task<Result<Response.GetClinicDetail>> Handle(Query.GetClinicDetailQuery request,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepository.FindByIdAsync(request.id, cancellationToken);
        if (clinic == null || clinic.IsDeleted)
        {
            return Result.Failure<Response.GetClinicDetail>(new Error("404", "Clinic not found."));
        }

        var branches = (currentUserService.Role == Constant.Role.CLINIC_ADMIN)
            ? await clinicRepository.FindAll(x => x.ParentId == request.id).ToListAsync(cancellationToken)
            : [];

        var branchDetails = branches.Select(x => MapToResponse(x)).ToList();

        var result = MapToResponse(clinic, branchDetails);
        return Result.Success(result);
    }

    private static Response.GetClinicDetail MapToResponse(Clinic clinic, List<Response.GetClinicDetail>? branches = null)
    {
        return new Response.GetClinicDetail(
            clinic.Id,
            clinic.Name,
            clinic.Email,
            clinic.PhoneNumber,
            clinic.Address,
            clinic.TaxCode,
            clinic.BusinessLicenseUrl,
            clinic.OperatingLicenseUrl,
            clinic.OperatingLicenseExpiryDate,
            clinic.ProfilePictureUrl,
            clinic.TotalBranches ?? 0,
            clinic.IsActivated,
            branches);
    }
}
