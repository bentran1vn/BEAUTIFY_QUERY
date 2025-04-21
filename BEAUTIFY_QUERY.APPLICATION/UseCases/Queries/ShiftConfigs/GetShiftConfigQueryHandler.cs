using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Documents.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.ShiftConfigs;

public class GetShiftConfigQueryHandler: IQueryHandler<Query.GetShiftConfigQuery, PagedResult<Response.ShiftResponse>>
{
    private readonly IRepositoryBase<ShiftConfig, Guid> _shiftConfigRepository;
    private readonly IRepositoryBase<Clinic, Guid> _clinicRepository;

    public GetShiftConfigQueryHandler(IRepositoryBase<ShiftConfig, Guid> shiftConfigRepository, IRepositoryBase<Clinic, Guid> clinicRepository)
    {
        _shiftConfigRepository = shiftConfigRepository;
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<PagedResult<Response.ShiftResponse>>> Handle(Query.GetShiftConfigQuery request, CancellationToken cancellationToken)
    {
        var query = _shiftConfigRepository.FindAll(x =>
            x.IsDeleted == false);

        if (request.RoleName.Equals(Constant.Role.CLINIC_ADMIN))
        {
            query = query.Where(x => x.ClinicId == request.ClinicId);
        }
        else
        {
            var clinicMain = await _clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);
            
            if (clinicMain == null)
            {
                return Result.Failure<PagedResult<Response.ShiftResponse>>(new Error("404", "Clinic not found"));
            }
            
            query = query.Where(x => x.ClinicId == clinicMain.ParentId);
        }
        
        var pageList = await PagedResult<ShiftConfig>.CreateAsync(query, request.PageNumber, request.PageSize);
        
        var result = PagedResult<Response.ShiftResponse>.Create(
            pageList.Items.Select(x => new Response.ShiftResponse(
                x.Id, x.Name, x.Note, x.StartTime,
                x.EndTime, x.CreatedOnUtc)
            ).ToList(),
            pageList.PageIndex, pageList.PageSize, pageList.TotalCount
        );

        return Result.Success(result);
    }
}