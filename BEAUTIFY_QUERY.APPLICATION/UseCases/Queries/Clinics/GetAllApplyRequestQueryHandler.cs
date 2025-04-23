using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class
    GetAllApplyRequestQueryHandler : IQueryHandler<Query.GetAllApplyRequestQuery, PagedResult<Response.GetApplyRequest>>
{
    private readonly IRepositoryBase<ClinicOnBoardingRequest, Guid> _clinicOnBoardingRequestRepository;

    public GetAllApplyRequestQueryHandler(
        IRepositoryBase<ClinicOnBoardingRequest, Guid> clinicOnBoardingRequestRepository)
    {
        _clinicOnBoardingRequestRepository = clinicOnBoardingRequestRepository;
    }

    public async Task<Result<PagedResult<Response.GetApplyRequest>>> Handle(Query.GetAllApplyRequestQuery request,
        CancellationToken cancellationToken)
    {
        var requestQuery = _clinicOnBoardingRequestRepository
                .FindAll(x => !x.IsDeleted && x.Status == 0 && x.IsMain, x => x.Clinic!)
                .OrderByDescending(x => x.ModifiedOnUtc).ThenByDescending(x => x.CreatedOnUtc)
            ;

        var applyRequest = await PagedResult<ClinicOnBoardingRequest>
            .CreateAsync(requestQuery, request.PageIndex, request.PageSize);

        var result = new PagedResult<Response.GetApplyRequest>(applyRequest.Items
                .Select(x =>
                    new Response.GetApplyRequest(x.Id, x.Clinic!.Name, x.Clinic.Email, x.Clinic.FullAddress,
                        x.Clinic.TotalApply, x.CreatedOnUtc)).ToList(), applyRequest.PageIndex, applyRequest.PageSize,
            applyRequest.TotalCount);

        return Result.Success(result);
    }
}