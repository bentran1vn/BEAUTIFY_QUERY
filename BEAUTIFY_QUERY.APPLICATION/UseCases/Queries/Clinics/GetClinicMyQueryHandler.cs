using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetClinicMyQueryHandler : IQueryHandler<Query.GetClinicMyQuery, Response.MyClinicApply>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;

    public GetClinicMyQueryHandler(IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }

    public async Task<Result<Response.MyClinicApply>> Handle(Query.GetClinicMyQuery request,
        CancellationToken cancellationToken)
    {
        if (request.RoleName != "Clinic Admin")
            return Result.Failure<Response.MyClinicApply>(new Error("403",
                "You do not have permission to access this resource."));

        var lastestRequest = await _clinicOnBoardingRequestRepository
            .FindAll(x => x.ClinicId == request.ClinicId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Include(x => x.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastestRequest?.Clinic == null || lastestRequest.Clinic.IsDeleted)
            return Result.Failure<Response.MyClinicApply>(new Error("404", "Clinic not found."));

        var result = new Response.MyClinicApply();

        result.Name = lastestRequest.Clinic.Name;
        result.Email = lastestRequest.Clinic.Email;
        result.PhoneNumber = lastestRequest.Clinic.PhoneNumber;
        result.City = lastestRequest.Clinic.City ?? "";
        result.Address = lastestRequest.Clinic.Address ?? "";
        result.Address = lastestRequest.Clinic.FullAddress ?? "";
        result.District = lastestRequest.Clinic.District ?? "";
        result.Ward = lastestRequest.Clinic.Ward ?? "";
        result.ProfilePictureUrl = lastestRequest.Clinic.ProfilePictureUrl ?? "";
        result.BankName = lastestRequest.Clinic.BankName ?? "";
        result.BankAccountNumber = lastestRequest.Clinic.BankAccountNumber ?? "";
        result.TaxCode = lastestRequest.Clinic.TaxCode;
        result.BusinessLicense = lastestRequest.Clinic.BusinessLicenseUrl;
        result.OperatingLicense = lastestRequest.Clinic.OperatingLicenseUrl;
        result.OperatingLicenseExpiryDate = (DateTimeOffset)lastestRequest.Clinic.OperatingLicenseExpiryDate;
        result.RejectReason = lastestRequest.RejectReason ?? "";

        return Result.Success(result);
    }
}