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
        
        List<Procedure> proceduresToUpdate = new List<Procedure>();
        int? indexToAdd;
        
        var isExisted = isServiceExisted.Procedures?.FirstOrDefault(x => x.Id == createRequest.Id);
        
        if (isExisted != null)
        {
            if (isExisted.StepIndex != createRequest.StepIndex)
            {
                var proceduresUpdate = isServiceExisted.Procedures
                    ?.Where(x => x.StepIndex != createRequest.StepIndex).ToList() ?? new List<Procedure>();
                
                foreach (var item in proceduresUpdate)
                {
                    if (item.StepIndex < createRequest.StepIndex)
                        item.StepIndex += 1;
                    else
                        item.StepIndex -= 1;
                }
                
                proceduresToUpdate.AddRange(proceduresUpdate);
            }
            indexToAdd = createRequest.StepIndex;
        }
        else
        {
            indexToAdd = isServiceExisted.Procedures?.Any() == true ? isServiceExisted.Procedures?.Max(x => x.StepIndex) + 1 : 0;
        }
        
        var procedure = new Procedure()
        {
            Id = createRequest.Id,
            Name = createRequest.Name,
            Description = createRequest.Description,
            StepIndex = (int)indexToAdd,
            ProcedurePriceTypes = createRequest.procedurePriceTypes
                .Select(x => new ProcedurePriceType(x.Id, x.Name, x.Price,x.Duration,x.IsDefault)).ToList()
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