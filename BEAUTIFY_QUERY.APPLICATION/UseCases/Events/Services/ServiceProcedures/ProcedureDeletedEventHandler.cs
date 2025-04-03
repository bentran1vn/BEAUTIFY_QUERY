using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServiceProcedures;

public class ProcedureDeletedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository): ICommandHandler<DomainEvents.ProcedureDelete>
{
    public async Task<Result> Handle(DomainEvents.ProcedureDelete request, CancellationToken cancellationToken)
    {
        var deleteRequest = request.entity;

        var isServiceExisted = await clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(deleteRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {deleteRequest.ServiceId} not found");

        var procedures = isServiceExisted.Procedures?.ToList() ?? [];

        var existPro = procedures.FirstOrDefault(x => x.Id == deleteRequest.Id)!;
        
        isServiceExisted.Procedures = procedures.Select(x => x.StepIndex > existPro.StepIndex
            ? new Procedure()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StepIndex = x.StepIndex - 1,
                ProcedurePriceTypes = x.ProcedurePriceTypes.Select(y => new ProcedurePriceType(y.Id, y.Name, y.Price, y.Duration, y.IsDefault)).ToList()
            }
            : x).ToList();
        
        isServiceExisted.Procedures.Remove(existPro);

        isServiceExisted.MinPrice = deleteRequest.MinPrice;
        isServiceExisted.MaxPrice = deleteRequest.MaxPrice;

        isServiceExisted.DiscountMinPrice = deleteRequest.DiscountMinPrice;
        isServiceExisted.DiscountMaxPrice = deleteRequest.DiscountMaxPrice;

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}