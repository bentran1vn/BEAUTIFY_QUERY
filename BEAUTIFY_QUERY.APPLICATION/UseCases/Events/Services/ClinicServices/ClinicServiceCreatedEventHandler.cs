using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ClinicServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ClinicServices;
public class ClinicServiceCreatedEventHandler : ICommandHandler<DomainEvents.ClinicServiceCreated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicServiceCreatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicServiceCreated request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        var service = new ClinicServiceProjection
        {
            DocumentId = serviceRequest.Id,
            Name = serviceRequest.Name,
            Description = serviceRequest.Description,
            DepositPercent = serviceRequest.DepositPercent,
            IsRefundable = serviceRequest.IsRefundable,
            Branding = new Clinic
            {
                Id = serviceRequest.Branding.Id,
                Name = serviceRequest.Branding.Name,
                Email = serviceRequest.Branding.Email,
                City =serviceRequest.Branding.City,
                Address = serviceRequest.Branding.Address,
                WorkingTimeStart = serviceRequest.Branding.WorkingTimeStart,
                WorkingTimeEnd = serviceRequest.Branding.WorkingTimeEnd,
                District = serviceRequest.Branding.District,
                Ward = serviceRequest.Branding.Ward,
                FullAddress = serviceRequest.Branding.FullAddress,
                PhoneNumber =  serviceRequest.Branding.PhoneNumber,
                ProfilePictureUrl = serviceRequest.Branding.ProfilePictureUrl,
                IsParent = serviceRequest.Branding.IsParent,
                IsActivated = true,
                ParentId = serviceRequest.Branding.ParentId
            },
            CoverImage = serviceRequest.CoverImages.Select(x => new Image
            {
                Id = x.Id,
                Index = x.Index,
                Url = x.Url
            }).ToList(),
            Category = new Category(
                serviceRequest.Category.Id, serviceRequest.Category.Name,
                serviceRequest.Category.Description
            ),
            Clinic = serviceRequest.Clinic.Select(x => new Clinic
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
                    ParentId = x.ParentId
                }).ToList(),
            Procedures = []
        };

        await _clinicServiceRepository.InsertOneAsync(service);

        return Result.Success();
    }
}