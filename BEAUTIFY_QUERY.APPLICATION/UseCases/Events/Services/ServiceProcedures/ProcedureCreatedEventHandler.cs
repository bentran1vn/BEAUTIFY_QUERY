using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Procedures;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServiceProcedures;

public class ProcedureCreatedEventHandler: ICommandHandler<DomainEvents.ProcedureCreated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ProcedureCreatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ProcedureCreated request, CancellationToken cancellationToken)
    {
        var createRequest = request.entity;

        var isServiceExisted = await _clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(createRequest.ServiceId));
        
        if(isServiceExisted == null) throw new Exception($"Service {createRequest.ServiceId} not found");

        var procedure = new Procedure(
            createRequest.Id,
            createRequest.Description,
            createRequest.Name,
            createRequest.StepIndex,
            createRequest.coverImage,
            createRequest.procedurePriceTypes.Select(x => new ProcedurePriceType(x.Id, x.Name, x.Price)).ToList());
        
        var procedures = isServiceExisted.Procedures?.ToList() ?? new List<Procedure>();
        
        procedures.Add(procedure);
        
        isServiceExisted.Procedures = procedures;
        
        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);
        
        return Result.Success();
    }
}