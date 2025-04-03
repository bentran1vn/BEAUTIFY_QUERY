using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ClinicServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ClinicServices;

public class ClinicServiceDeletedEventHandler: ICommandHandler<DomainEvents.ClinicServiceDeleted>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;
    
    public ClinicServiceDeletedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }
    
    public async Task<Result> Handle(DomainEvents.ClinicServiceDeleted request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

       await _clinicServiceRepository.DeleteOneAsync(x => x.DocumentId == serviceRequest.Id);
        
       return Result.Success();
    }
}