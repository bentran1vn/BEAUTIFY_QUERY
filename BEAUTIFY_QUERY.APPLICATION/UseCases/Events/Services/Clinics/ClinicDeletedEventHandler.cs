using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Clinic;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Clinics;

public class ClinicDeletedEventHandler: ICommandHandler<DomainEvents.ClinicDeleted>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicDeletedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicDeleted request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;
        
        var isServiceExisted = await _clinicServiceRepository
            .AsQueryable(x => x.Branding.Id.Equals(serviceRequest.ParentId))
            .ToListAsync(cancellationToken);
        
        if (isServiceExisted != null)
        {
            if (!serviceRequest.IsParent)
            {
                var updateTasks = new List<Task>();
                
                foreach (var item in isServiceExisted)
                {
                    // Fix: Remove the clinic with matching ID instead of keeping only it
                    item.Clinic = item.Clinic.Where(x => !x.Id.Equals(serviceRequest.ClinicId)).ToList();
                    updateTasks.Add(_clinicServiceRepository.ReplaceOneAsync(item));
                }
                
                // Wait for all update tasks to complete in parallel
                await Task.WhenAll(updateTasks);
            }
        }

        return Result.Success();
    }
}