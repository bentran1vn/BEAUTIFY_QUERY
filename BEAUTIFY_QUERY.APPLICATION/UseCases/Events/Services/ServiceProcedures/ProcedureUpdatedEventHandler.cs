using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServiceProcedures;

public class ProcedureUpdatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    : ICommandHandler<DomainEvents.ProcedureUpdate>
{
    public async Task<Result> Handle(DomainEvents.ProcedureUpdate request, CancellationToken cancellationToken)
    {
        var createRequest = request.entity;

        var isServiceExisted = await clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(createRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {createRequest.ServiceId} not found");
        
        isServiceExisted.Procedures = isServiceExisted.Procedures
            .Select(x => x.Id.Equals(createRequest.Id) ? new Procedure(
                createRequest.Id,
                createRequest.Description,
                createRequest.Name,
                createRequest.StepIndex,
                createRequest.procedurePriceTypes.Select(y => new ProcedurePriceType(y.Id, y.Name, y.Price, y.Duration, y.IsDefault)).ToList())
                : x)
            .ToList();
        
        isServiceExisted.MinPrice = createRequest.MinPrice;
        isServiceExisted.MaxPrice = createRequest.MaxPrice;

        isServiceExisted.DiscountMinPrice = createRequest.DiscountMinPrice ?? createRequest.MinPrice;
        isServiceExisted.DiscountMaxPrice = createRequest.DiscountMaxPrice ?? createRequest.MaxPrice;

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}