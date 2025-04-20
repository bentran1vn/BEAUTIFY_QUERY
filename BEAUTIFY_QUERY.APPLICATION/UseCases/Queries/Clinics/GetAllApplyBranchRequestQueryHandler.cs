using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;

public class GetAllApplyBranchRequestQueryHandler : IQueryHandler<Query.GetAllApplyBranchRequestQuery, PagedResult<Response.BranchClinicApplyGetAll>>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;

    public GetAllApplyBranchRequestQueryHandler(IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }

    public async Task<Result<PagedResult<Response.BranchClinicApplyGetAll>>> Handle(Query.GetAllApplyBranchRequestQuery request, CancellationToken cancellationToken)
    {
        var requestQuery = _clinicOnBoardingRequestRepository
            .FindAll(x => x.IsDeleted == false && !x.IsMain);

        requestQuery = requestQuery
            .Include(x => x.Clinic)
                .ThenInclude(x => x.Parent);
        
        if(request.ClinicId != null)
        {
            requestQuery = requestQuery.Where(x => x.Status == 0 || x.Status == 1);
            requestQuery = requestQuery.Where(x => x.Clinic!.ParentId == request.ClinicId);
        }
        else
        {
            requestQuery = requestQuery.Where(x => x.Status == 0);
        }

        var applyRequest = await PagedResult<ClinicOnBoardingRequest>
            .CreateAsync(requestQuery, request.PageIndex, request.PageSize);
        
        var result = PagedResult<Response.BranchClinicApplyGetAll>.Create(
            applyRequest.Items.Select(x =>
                new Response.BranchClinicApplyGetAll()
                {
                    Id = x.Id,
                    Name = x.Clinic!.Name,
                    CreatedOnUtc = x.CreatedOnUtc,
                    ParentId = (Guid)x.Clinic.ParentId,
                    ParentName = x.Clinic.Parent!.Name,
                    ParentEmail = x.Clinic.Parent.Email,
                    ParentAddress = x.Clinic.Parent.FullAddress ?? "",
                    RejectReason = x.RejectReason
                }).ToList(), applyRequest.PageIndex, applyRequest.PageSize,
            applyRequest.TotalCount);

        return Result.Success(result);
    }
}