using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Clinic;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Clinics;

public class ClinicUpdatedEventHandler: ICommandHandler<DomainEvents.ClinicUpdated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicUpdatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicUpdated request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;
        
        if (serviceRequest.IsParent)
        {
            // Extract ID to a variable to avoid dynamic operation in LINQ expression
            var clinicId = serviceRequest.Clinic.Id;
            
            var isServiceExisted = await _clinicServiceRepository
                .AsQueryable(x => x.Branding.Id.Equals(clinicId))
                .ToListAsync(cancellationToken);

            if (isServiceExisted != null && isServiceExisted.Any())
            {
                var updateTasks = new List<Task>();
                
                foreach (var item in isServiceExisted)
                {
                    item.Branding = new Clinic
                    {
                        Id = serviceRequest.Clinic.Id,
                        Name = serviceRequest.Clinic.Name,
                        Email = serviceRequest.Clinic.Email,
                        City = serviceRequest.Clinic.City,
                        Address = serviceRequest.Clinic.Address,
                        District = serviceRequest.Clinic.District,
                        WorkingTimeStart = serviceRequest.Clinic.WorkingTimeStart,
                        WorkingTimeEnd = serviceRequest.Clinic.WorkingTimeEnd,
                        Ward = serviceRequest.Clinic.Ward,
                        FullAddress = serviceRequest.Clinic.FullAddress,
                        PhoneNumber = serviceRequest.Clinic.PhoneNumber,
                        ProfilePictureUrl = serviceRequest.Clinic.ProfilePictureUrl,
                        IsParent = serviceRequest.Clinic.IsParent,
                        IsActivated = true,
                        ParentId = serviceRequest.Clinic.ParentId
                    };
                    
                    updateTasks.Add(_clinicServiceRepository.ReplaceOneAsync(item));
                }
                
                // Run all updates in parallel
                await Task.WhenAll(updateTasks);
            }
        }
        else
        {
            // Extract ID to a variable to avoid dynamic operation in LINQ expression
            var parentId = serviceRequest.Clinic.ParentId;
            
            var isServiceExisted = await _clinicServiceRepository
                .AsQueryable(x => x.Branding.Id.Equals(parentId))
                .ToListAsync(cancellationToken);

            if (isServiceExisted != null && isServiceExisted.Any())
            {
                var updateTasks = new List<Task>();
                
                foreach (var item in isServiceExisted)
                {
                    item.Clinic = item.Clinic.Select(x =>
                    {
                        if (x.Id.Equals(serviceRequest.Clinic.Id))
                        {
                            return new Clinic
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Email = x.Email,
                                City = x.City,
                                Address = x.Address,
                                District = x.District,
                                WorkingTimeStart = x.WorkingTimeStart,
                                WorkingTimeEnd = x.WorkingTimeEnd,
                                Ward = x.Ward,
                                FullAddress = x.FullAddress,
                                PhoneNumber = x.PhoneNumber,
                                ProfilePictureUrl = x.ProfilePictureUrl,
                                IsParent = x.IsParent,
                                IsActivated = true,
                                ParentId = x.ParentId,
                            };
                        }

                        return x;
                    }).ToList();
                    
                    updateTasks.Add(_clinicServiceRepository.ReplaceOneAsync(item));
                }
                
                // Run all updates in parallel
                await Task.WhenAll(updateTasks);
            }
        }
        
        return Result.Success();
    }
}