using BEAUTIFY_QUERY.CONTRACT.Services.Services;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;
internal sealed class GetAllServiceInGetClinicByIdQueryHandler(
    ICurrentUserService currentUserService,
    IMongoRepository<ClinicServiceProjection> clinicServiceProjectionRepository)
    : IQueryHandler<Query.GetAllServiceInGetClinicByIdQuery, PagedResult<Response.GetAllServiceInGetClinicById>>
{
    public async Task<Result<PagedResult<Response.GetAllServiceInGetClinicById>>> Handle(
        Query.GetAllServiceInGetClinicByIdQuery request, CancellationToken cancellationToken)
    {
        var query = clinicServiceProjectionRepository.AsQueryable(x =>
            x.Clinic.Any(x => x.Id.Equals(currentUserService.ClinicId)));

        var result = await PagedResult<ClinicServiceProjection>.CreateAsyncMongoLinq(query,
            request.PageNumber, request.PageSize);

        var mapList = result.Items.Select(x => new Response.GetAllServiceInGetClinicById
        {
            Id = x.DocumentId,
            Name = x.Name,
            Description = x.Description,
            MaxPrice = x.MaxPrice,
            MinPrice = x.MinPrice,
            DepositPercent = x.DepositPercent,
            IsRefundable = x.IsRefundable,
            DiscountPercent = (x.DiscountPercent * 100).ToString(),
            DiscountMaxPrice = x.DiscountMaxPrice,
            DiscountMinPrice = x.DiscountMinPrice,
            CoverImage = x.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList()
        }).ToList();

        return Result.Success(new PagedResult<Response.GetAllServiceInGetClinicById>(mapList,
            result.PageIndex, result.PageSize, result.TotalCount));
    }
}