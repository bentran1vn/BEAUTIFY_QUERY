using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetDetailApplyRequestQueryHandler : IQueryHandler<Query.GetDetailApplyRequestQuery, Response.GetApplyRequestById>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;

    public GetDetailApplyRequestQueryHandler(IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }

    public async Task<Result<Response.GetApplyRequestById>> Handle(Query.GetDetailApplyRequestQuery request, CancellationToken cancellationToken)
    {
        var requestDetail = await _clinicOnBoardingRequestRepository.FindByIdAsync(request.ApplyRequestId, cancellationToken, x => x.Clinic!);
        if (requestDetail == null || requestDetail.IsDeleted)
        {
            return Result.Failure<Response.GetApplyRequestById>(new Error("404", "Clinic Apply Request Not Found"));
        }
        var result = new Response.GetApplyRequestById(requestDetail.Id, requestDetail.Clinic!.Name, requestDetail.Clinic!.Email,
            requestDetail.Clinic!.PhoneNumber, requestDetail.Clinic!.Address, requestDetail.Clinic!.TaxCode,
            requestDetail.Clinic!.BusinessLicenseUrl, requestDetail.Clinic!.OperatingLicenseUrl,
            requestDetail.Clinic!.OperatingLicenseExpiryDate, requestDetail.Clinic!.ProfilePictureUrl, requestDetail.Clinic!.TotalApply
        );
        
        return Result.Success(result);
    }
}