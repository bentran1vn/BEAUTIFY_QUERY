using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServiceProcedures;
public class ProcedureCreatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    : ICommandHandler<DomainEvents.ProcedureCreated>
{
    public async Task<Result> Handle(DomainEvents.ProcedureCreated request, CancellationToken cancellationToken)
    {
        var createRequest = request.entity;

        var isServiceExisted = await clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(createRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {createRequest.ServiceId} not found");
        
        var isExisted = isServiceExisted.Procedures?.FirstOrDefault(
            x => x.StepIndex == createRequest.StepIndex
        );
        
        int? indexToAdd;
        List<Procedure> proceduresToUpdate = new List<Procedure>();
        
        if (isExisted != null)
        {
            var proceduresUpdate = isServiceExisted.Procedures?.Where(x => x.StepIndex >= createRequest.StepIndex).ToList() ?? [];
            foreach (var item in proceduresUpdate)
            {
                item.StepIndex += 1;
            }

            if (proceduresUpdate.Any())
                proceduresToUpdate.AddRange(proceduresUpdate);
            
            indexToAdd = createRequest.StepIndex;
        }
        else
        {
            var nextStepIndex = isServiceExisted.Procedures?.Any() == true ? isServiceExisted.Procedures?.Max(x => x.StepIndex) + 1 : 0;
            indexToAdd = nextStepIndex;
            var proceduresUpdate = isServiceExisted.Procedures?.Where(x => x.StepIndex != nextStepIndex).ToList() ?? [];
            proceduresToUpdate.AddRange(proceduresUpdate);
        }

        var procedure = new Procedure()
        {
            Id = Guid.NewGuid(),
            Name = createRequest.Name,
            Description = createRequest.Description,
            StepIndex = (int)indexToAdd,
            ProcedurePriceTypes = createRequest.procedurePriceTypes.Select(x => new ProcedurePriceType(x.Id, x.Name, x.Price,x.Duration,x.IsDefault)).ToList()
        };
        
        proceduresToUpdate.Add(procedure);

        isServiceExisted.Procedures = proceduresToUpdate;

        isServiceExisted.MinPrice = createRequest.MinPrice;
        isServiceExisted.MaxPrice = createRequest.MaxPrice;

        isServiceExisted.DiscountMinPrice = createRequest.DiscountMinPrice ?? createRequest.MinPrice;
        isServiceExisted.DiscountMaxPrice = createRequest.DiscountMaxPrice ?? createRequest.MaxPrice;

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}