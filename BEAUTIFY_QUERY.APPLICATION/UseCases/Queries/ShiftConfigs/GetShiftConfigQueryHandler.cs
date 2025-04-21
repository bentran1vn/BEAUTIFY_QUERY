using BEAUTIFY_QUERY.CONTRACT.Services.ShiftConfigs;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.ShiftConfigs;

public class GetShiftConfigQueryHandler: IQueryHandler<Query.GetShiftConfigQuery, PagedResult<Response.ShiftResponse>>
{
    private readonly IRepositoryBase<ShiftConfig, Guid> shiftConfigRepository;

    public GetShiftConfigQueryHandler(IRepositoryBase<ShiftConfig, Guid> shiftConfigRepository)
    {
        this.shiftConfigRepository = shiftConfigRepository;
    }

    public async Task<Result<PagedResult<Response.ShiftResponse>>> Handle(Query.GetShiftConfigQuery request, CancellationToken cancellationToken)
    {
        var query = shiftConfigRepository.FindAll(x =>
            x.IsDeleted == false && x.ClinicId == request.ClinicId);
        
        var pageList = await PagedResult<ShiftConfig>.CreateAsync(query, request.PageNumber, request.PageSize);
        
        var result = PagedResult<Response.ShiftResponse>.Create(pageList.Items.Select(x => new Response.ShiftResponse
        (x.Id, x.Name, x.Note, x.StartTime, x.EndTime, x.CreatedOnUtc)).ToList(), pageList.PageIndex, pageList.PageSize, pageList.TotalCount);

        return Result.Success(result);
    }
}