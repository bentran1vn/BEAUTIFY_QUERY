using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServiceProcedures;

public class ProcedureDeletedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository): ICommandHandler<DomainEvents.ProcedureDelete>
{
    public async Task<Result> Handle(DomainEvents.ProcedureDelete request, CancellationToken cancellationToken)
    {
        var createRequest = request.entity;

        var isServiceExisted = await clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(createRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {createRequest.ServiceId} not found");

        var procedure = new Procedure(
            createRequest.Id,
            createRequest.Description,
            createRequest.Name,
            createRequest.StepIndex,
            createRequest.coverImage,
            createRequest.procedurePriceTypes.Select(x => new ProcedurePriceType(x.Id, x.Name, x.Price,x.Duration,x.IsDefault)).ToList());

        var procedures = isServiceExisted.Procedures?.ToList() ?? [];

        procedures.Add(procedure);

        isServiceExisted.Procedures = procedures;

        isServiceExisted.MinPrice = createRequest.MinPrice;
        isServiceExisted.MaxPrice = createRequest.MaxPrice;

        isServiceExisted.DiscountMinPrice = createRequest.DiscountMinPrice ?? createRequest.MinPrice;
        isServiceExisted.DiscountMaxPrice = createRequest.DiscountMaxPrice ?? createRequest.MaxPrice;

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}