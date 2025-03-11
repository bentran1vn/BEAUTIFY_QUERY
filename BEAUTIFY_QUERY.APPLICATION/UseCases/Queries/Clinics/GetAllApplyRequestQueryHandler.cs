using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetAllApplyRequestQueryHandler : IQueryHandler<Query.GetAllApplyRequestQuery, PagedResult<Response.GetApplyRequest>>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;
    public GetAllApplyRequestQueryHandler(IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }

    public async Task<Result<PagedResult<Response.GetApplyRequest>>> Handle(Query.GetAllApplyRequestQuery request, CancellationToken cancellationToken)
    {
        var requestQuery = _clinicOnBoardingRequestRepository
            .FindAll(x => x.IsDeleted == false && x.Status == 0, x => x.Clinic!);
        
        var applyRequest = await PagedResult<ClinicOnBoardingRequest>
            .CreateAsync(requestQuery, request.PageIndex, request.PageSize);

        PagedResult<Response.GetApplyRequest> result = new PagedResult<Response.GetApplyRequest>(applyRequest.Items
            .Select(x => new Response.GetApplyRequest(x.Id, x.Clinic!.Name, x.Clinic.Email, x.Clinic.Address, x.Clinic.TotalApply)).ToList(), applyRequest.PageIndex, applyRequest.PageSize, applyRequest.TotalCount);
        
        return Result.Success(result);
    }
}