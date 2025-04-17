using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Clinic;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Clinics;
public class ClinicBranchActivatedActionEventHandler : ICommandHandler<DomainEvents.ClinicBranchActivatedAction>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicBranchActivatedActionEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicBranchActivatedAction request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        var isServiceExisted = await _clinicServiceRepository
            .AsQueryable(x => x.Branding.Id.Equals(serviceRequest.ParentId))
            .ToListAsync(cancellationToken);

        if (isServiceExisted != null)
            foreach (var item in isServiceExisted)
            {
                if (serviceRequest.IsParent)
                    item.Branding.IsActivated = serviceRequest.IsActive;
                else
                    item.Clinic = item.Clinic.Select(x =>
                    {
                        if (x.Id.Equals(serviceRequest.Id)) x.IsActivated = serviceRequest.IsActive;

                        return x;
                    }).ToList();
                await _clinicServiceRepository.ReplaceOneAsync(item);
            }

        await _clinicServiceRepository.DeleteManyAsync(x => x.Branding.Id.Equals(serviceRequest.ParentId));
        if (isServiceExisted != null) await _clinicServiceRepository.InsertManyAsync(isServiceExisted);

        return Result.Success();
    }
}