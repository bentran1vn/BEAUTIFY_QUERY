using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetDetailBranchApplyRequestQueryHandler: IQueryHandler<Query.GetDetailBranchApplyRequestQuery, Response.BranchClinicApplyDetail>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;
    
    public GetDetailBranchApplyRequestQueryHandler(
        IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }
    
    public async Task<Result<Response.BranchClinicApplyDetail>> Handle(Query.GetDetailBranchApplyRequestQuery request, CancellationToken cancellationToken)
    {
        var requestQuery = _clinicOnBoardingRequestRepository
            .FindAll(x =>x.Id.Equals(request.ApplyRequestId) &&  x.IsDeleted == false && !x.IsMain);

        requestQuery = requestQuery
            .Include(x => x.Clinic)
            .ThenInclude(x => x.Parent);

        var requestDetail = await requestQuery.FirstOrDefaultAsync(cancellationToken);

        if (requestDetail == null || requestDetail.IsDeleted)
            return Result.Failure<Response.BranchClinicApplyDetail>(new Error("404", "Clinic Apply Request Not Found"));

        var result = new Response.BranchClinicApplyDetail()
        {
            Id = requestDetail.Id,
            Name = requestDetail.Clinic!.Name,
            Email = requestDetail.Clinic.Email,
            PhoneNumber = requestDetail.Clinic.PhoneNumber,
            City = requestDetail.Clinic.City,
            Address = requestDetail.Clinic.Address,
            District = requestDetail.Clinic.District,
            Ward = requestDetail.Clinic.Ward,
            FullAddress = requestDetail.Clinic.FullAddress,
            TaxCode = requestDetail.Clinic.TaxCode,
            WorkingTimeStart = requestDetail.Clinic.WorkingTimeStart,
            WorkingTimeEnd = requestDetail.Clinic.WorkingTimeEnd,
            BankName = requestDetail.Clinic.BankName,
            BankAccountNumber = requestDetail.Clinic.BankAccountNumber,
            BusinessLicenseUrl = requestDetail.Clinic.BusinessLicenseUrl,
            OperatingLicenseUrl = requestDetail.Clinic.OperatingLicenseUrl,
            OperatingLicenseExpiryDate = requestDetail.Clinic.OperatingLicenseExpiryDate,
            ProfilePictureUrl = requestDetail.Clinic.ProfilePictureUrl,
            CreatedOnUtc = requestDetail.CreatedOnUtc,
            ParentId = (Guid)requestDetail.Clinic.ParentId,
            ParentName = requestDetail.Clinic.Parent!.Name,
            ParentEmail = requestDetail.Clinic.Parent.Email,
            ParentAddress = requestDetail.Clinic.Parent.FullAddress ?? "",
            ParentPhoneNumber = requestDetail.Clinic.Parent.PhoneNumber,
            ParentCity = requestDetail.Clinic.Parent.City,
            ParentDistrict = requestDetail.Clinic.Parent.District,
            ParentWard = requestDetail.Clinic.Parent.Ward,
            RejectReason = requestDetail.RejectReason
        };

        return Result.Success(result);
    }
}