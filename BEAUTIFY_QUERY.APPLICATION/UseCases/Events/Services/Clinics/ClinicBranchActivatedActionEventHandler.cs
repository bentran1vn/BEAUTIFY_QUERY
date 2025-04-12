using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Clinic;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Clinics;

public class ClinicBranchActivatedActionEventHandler: ICommandHandler<DomainEvents.ClinicBranchActivatedAction>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicBranchActivatedActionEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicBranchActivatedAction request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;
        
        var isServiceExisted = await _clinicServiceRepository
                                   .FindOneAsync(p => p.DocumentId == serviceRequest.Id)
                               ?? throw new Exception($"Service {serviceRequest.Id} not found");

        
        
        if (serviceRequest.IsParent)
        {
            isServiceExisted.Branding.IsActivated = serviceRequest.IsActive;
        }
        else
        {
            isServiceExisted.Clinic = isServiceExisted.Clinic.Select(x =>
            {
                if (x.Id.Equals(serviceRequest.Id))
                {
                    x.IsActivated = serviceRequest.IsActive;
                }

                return x;
            }).ToList();
        }
        
        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);
        
        return Result.Success();
    }
}