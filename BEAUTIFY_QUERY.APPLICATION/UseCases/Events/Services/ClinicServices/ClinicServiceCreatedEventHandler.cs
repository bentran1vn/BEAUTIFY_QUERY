using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
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
        
        var service = new ClinicServiceProjection()
        {
            DocumentId = serviceRequest.Id,
            Name = serviceRequest.Name,
            Description = serviceRequest.Description,
            CoverImage = serviceRequest.CoverImage,
            DescriptionImage = serviceRequest.DescriptionImage,
            Price = serviceRequest.Price,
            Category = new Category(
                serviceRequest.Category.Id,serviceRequest.Category.Name,
                serviceRequest.Category.Description
            ),
            Clinic = new Clinic(
                serviceRequest.Clinic.Id, serviceRequest.Clinic.Name,
                serviceRequest.Clinic.Email, serviceRequest.Clinic.Address,
                serviceRequest.Clinic.PhoneNumber, serviceRequest.Clinic.ProfilePictureUrl),
            Procedures = []
        };
        
        await _clinicServiceRepository.InsertOneAsync(service);
        
        return Result.Success();
    }
}