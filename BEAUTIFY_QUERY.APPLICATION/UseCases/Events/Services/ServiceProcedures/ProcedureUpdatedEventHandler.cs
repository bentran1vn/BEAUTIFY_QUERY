using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using Procedure = BEAUTIFY_QUERY.DOMAIN.Documents.Procedure;
using ProcedurePriceType = BEAUTIFY_QUERY.DOMAIN.Documents.ProcedurePriceType;

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

        var proceduresToUpdate = new List<Procedure>();

        var isExisted = isServiceExisted.Procedures?.FirstOrDefault(x => x.Id == createRequest.Id);

        if (isExisted == null)
            throw new Exception($"Procedure {createRequest.Id} not found");

        if (isServiceExisted.Procedures != null &&
            (isServiceExisted.Procedures.Max(x => x.StepIndex) < createRequest.StepIndex ||
             createRequest.StepIndex < isServiceExisted.Procedures.Min(x => x.StepIndex)))
            return Result.Failure(new Error("400", "Step index is out of range !"));

        var indexToAdd = isExisted.StepIndex;

        if (isExisted.StepIndex != createRequest.StepIndex)
        {
            // Jump back
            if (isExisted.StepIndex > createRequest.StepIndex)
            {
                var proceduresToUpdateTop = isServiceExisted.Procedures?
                    .Where(x =>
                        x.StepIndex >= createRequest.StepIndex &&
                        x.StepIndex < isExisted.StepIndex).ToList();

                if (proceduresToUpdateTop != null && proceduresToUpdateTop.Any())
                    proceduresToUpdateTop = proceduresToUpdateTop
                        .Select(x => new Procedure
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            StepIndex = x.StepIndex + 1,
                            ProcedurePriceTypes = x.ProcedurePriceTypes
                        }).ToList();

                var procedureTop = isServiceExisted.Procedures?
                    .Where(x => x.StepIndex > isExisted.StepIndex).ToList();

                var proceduresDown = isServiceExisted.Procedures?
                    .Where(x => x.StepIndex < createRequest.StepIndex).ToList();

                if (proceduresToUpdateTop != null && proceduresToUpdateTop.Any())
                    proceduresToUpdate.AddRange(proceduresToUpdateTop);

                if (procedureTop != null && procedureTop.Any())
                    proceduresToUpdate.AddRange(procedureTop);

                if (proceduresDown != null && proceduresDown.Any())
                    proceduresToUpdate.AddRange(proceduresDown);
            }

            // Jump next
            if (isExisted.StepIndex < createRequest.StepIndex)
            {
                var proceduresToUpdateDown = isServiceExisted.Procedures?
                    .Where(x =>
                        x.StepIndex <= createRequest.StepIndex &&
                        x.StepIndex > isExisted.StepIndex).ToList();

                if (proceduresToUpdateDown != null && proceduresToUpdateDown.Any())
                    proceduresToUpdateDown = proceduresToUpdateDown
                        .Select(x => new Procedure
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            StepIndex = x.StepIndex - 1,
                            ProcedurePriceTypes = x.ProcedurePriceTypes
                        }).ToList();

                var proceduresTop = isServiceExisted.Procedures?
                    .Where(x => x.StepIndex > createRequest.StepIndex).ToList();

                var procedureDown = isServiceExisted.Procedures?.Where(x => x.StepIndex < isExisted.StepIndex).ToList();

                if (proceduresToUpdateDown != null && proceduresToUpdateDown.Any())
                    proceduresToUpdate.AddRange(proceduresToUpdateDown);

                if (procedureDown != null && procedureDown.Any())
                    proceduresToUpdate.AddRange(procedureDown);

                if (proceduresTop != null && proceduresTop.Any())
                    proceduresToUpdate.AddRange(proceduresTop);
            }

            indexToAdd = createRequest.StepIndex;
        }
        else
        {
            var proceduresUpdate = isServiceExisted.Procedures?
                .Where(x => x.StepIndex != createRequest.StepIndex).ToList();

            if (proceduresUpdate != null && proceduresUpdate.Any())
                proceduresToUpdate.AddRange(proceduresUpdate);
        }

        var procedure = new Procedure
        {
            Id = createRequest.Id,
            Name = createRequest.Name,
            Description = createRequest.Description,
            StepIndex = indexToAdd,
            ProcedurePriceTypes = createRequest.procedurePriceTypes
                .Select(x => new ProcedurePriceType(x.Id, x.Name, x.Price, x.Duration, x.IsDefault)).ToList()
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