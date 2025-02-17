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


        var result = new Response.GetAllServiceByIdResponse(
            isServiceExisted.DocumentId, isServiceExisted.Name, isServiceExisted.Description,
            isServiceExisted.Price, isServiceExisted.CoverImage, isServiceExisted.DescriptionImage,
            new Response.Clinic(
                isServiceExisted.Clinic.Id, isServiceExisted.Clinic.Name, isServiceExisted.Clinic.Email,
                isServiceExisted.Clinic.Address, isServiceExisted.Clinic.PhoneNumber,
                isServiceExisted.Clinic.ProfilePictureUrl),
            new Response.Category(isServiceExisted.Category.Id, isServiceExisted.Category.Name,
                isServiceExisted.Category.Description),
            isServiceExisted.Procedures.Select(x => new Response.Procedure(
                    x.Id, x.Name, x.Description, x.StepIndex, x.coverImage,
                    x.procedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                        y.Id, y.Name, y.Price)).ToList()
                )).ToList()
        );
        
        return Result.Success(result);
    }
}