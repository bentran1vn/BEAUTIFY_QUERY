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
            Branding = new Clinic(
                serviceRequest.Branding.Id, serviceRequest.Branding.Name, serviceRequest.Branding.Email,
                serviceRequest.Branding.City, serviceRequest.Branding.Address, serviceRequest.Branding.District,
                serviceRequest.Branding.Ward, serviceRequest.Branding.FullAddress, serviceRequest.Branding.PhoneNumber,
                serviceRequest.Branding.ProfilePictureUrl, serviceRequest.Branding.IsParent, true, serviceRequest.Branding.ParentId),
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
            Clinic = serviceRequest.Clinic.Select(x => new Clinic(
                x.Id, x.Name, x.Email, x.City, x.Address, x.District, x.Ward, x.FullAddress, x.PhoneNumber,
                x.ProfilePictureUrl, x.IsParent, true, x.ParentId)).ToList(),
            Procedures = []
        };

        await _clinicServiceRepository.InsertOneAsync(service);

        return Result.Success();
    }
}