using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetClinicServicesByIdQueryHandler: IQueryHandler<Query.GetClinicServicesByIdQuery, Response.GetAllServiceByIdResponse>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public GetClinicServicesByIdQueryHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result<Response.GetAllServiceByIdResponse>> Handle(Query.GetClinicServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var isServiceExisted = await _clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(request.ServiceId));
        
        if(isServiceExisted == null) throw new Exception($"Service {request.ServiceId} not found");

        Response.GetAllServiceByIdResponse result;
        
        result = new Response.GetAllServiceByIdResponse(
            isServiceExisted.DocumentId, isServiceExisted.Name, isServiceExisted.Description,
            isServiceExisted.MaxPrice, isServiceExisted.MinPrice, (isServiceExisted.DiscountPercent * 100).ToString(),
            isServiceExisted.DiscountMaxPrice, isServiceExisted.DiscountMinPrice,
            isServiceExisted.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
            isServiceExisted.DescriptionImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
            isServiceExisted.Clinic.Select(y => new Response.Clinic(y.Id, y.Name, y.Email,
                y.Address, y.PhoneNumber, y.ProfilePictureUrl, y.IsParent, y.ParentId)).ToList(),
            new Response.Category(isServiceExisted.Category.Id, isServiceExisted.Category.Name,
                isServiceExisted.Category.Description),
            isServiceExisted.Procedures.Select(x => new Response.Procedure(
                x.Id, x.Name, x.Description, x.StepIndex, x.coverImage,
                x.procedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                    y.Id, y.Name, y.Price)).ToList()
            )).ToList(), null
        );
        
        if (request.MainClinicId != null)
        {
            result = new Response.GetAllServiceByIdResponse(
                isServiceExisted.DocumentId, isServiceExisted.Name, isServiceExisted.Description,
                isServiceExisted.MaxPrice, isServiceExisted.MinPrice, (isServiceExisted.DiscountPercent * 100).ToString(),
                isServiceExisted.DiscountMaxPrice, isServiceExisted.DiscountMinPrice,
                isServiceExisted.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
                isServiceExisted.DescriptionImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
                isServiceExisted.Clinic.Select(y => new Response.Clinic(y.Id, y.Name, y.Email,
                    y.Address, y.PhoneNumber, y.ProfilePictureUrl, y.IsParent, y.ParentId)).ToList(),
                new Response.Category(isServiceExisted.Category.Id, isServiceExisted.Category.Name,
                    isServiceExisted.Category.Description),
                isServiceExisted.Procedures.Select(x => new Response.Procedure(
                    x.Id, x.Name, x.Description, x.StepIndex, x.coverImage,
                    x.procedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                        y.Id, y.Name, y.Price)).ToList()
                )).ToList(), isServiceExisted.Promotions.Select(x => 
                    new Response.Promotion(
                        x.Id,
                        x.Name,
                        x.DiscountPercent,
                        x.ImageUrl,
                        x.StartDay,
                        x.EndDate,
                        x.IsActivated
                    )).ToList()
            );
        }
        
        return Result.Success(result);
    }
}