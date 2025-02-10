using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetClinicDetailQueryHandler :  IQueryHandler<Query.GetClinicDetailQuery, Response.GetClinicDetail>
{
    private readonly IRepositoryBase<DOMAIN.Entities.Clinic, Guid> _clinicRepository;

    public GetClinicDetailQueryHandler(IRepositoryBase<DOMAIN.Entities.Clinic, Guid> clinicRepository)
    {
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<Response.GetClinicDetail>> Handle(Query.GetClinicDetailQuery request, CancellationToken cancellationToken)
    {
        var clinic = await _clinicRepository.FindByIdAsync(request.id, cancellationToken);

        if (clinic == null || clinic.IsDeleted)
        {
            return Result.Failure<Response.GetClinicDetail>(new Error("404", "Clinic not found."));
        }
        
        var result = new Response.GetClinicDetail(clinic.Id, clinic.Name, clinic.Email, clinic.PhoneNumber,
            clinic.Address, clinic.TaxCode, clinic.BusinessLicenseUrl, clinic.OperatingLicenseUrl,
            clinic.OperatingLicenseExpiryDate, clinic.ProfilePictureUrl, clinic.TotalBranches ?? 0, clinic.IsActivated);
        
        return Result.Success(result);
    }
}