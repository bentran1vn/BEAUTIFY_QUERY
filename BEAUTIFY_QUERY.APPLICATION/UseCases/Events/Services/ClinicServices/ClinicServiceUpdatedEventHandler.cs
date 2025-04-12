using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ClinicServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.EntityEvents;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ClinicServices;
public class ClinicServiceUpdatedEventHandler : ICommandHandler<DomainEvents.ClinicServiceUpdated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ClinicServiceUpdatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ClinicServiceUpdated request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        // Fetch the existing service (throw exception if not found)
        var isServiceExisted = await _clinicServiceRepository
                                   .FindOneAsync(p => p.DocumentId == serviceRequest.Id)
                               ?? throw new Exception($"Service {serviceRequest.Id} not found");

        isServiceExisted.Name = serviceRequest.Name;
        isServiceExisted.Description = serviceRequest.Description;
        isServiceExisted.Category = new Category(
            serviceRequest.Category.Id, serviceRequest.Category.Name,
            serviceRequest.Category.Description
        );
        isServiceExisted.Clinic = serviceRequest.Clinic.Select(x => new Clinic(
            x.Id, x.Name, x.Email, x.City, x.Address, x.District, x.Ward, x.FullAddress, x.PhoneNumber,
            x.ProfilePictureUrl, x.IsParent, true, x.ParentId)).ToList();

        isServiceExisted.CoverImage = serviceRequest.CoverImages
            .Select(x => new Image()
            {
                Id = x.Id,
                Index = x.Index,
                Url = x.Url
            }).ToList();
            

        // Save updated service back to the database
        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }

    /// <summary>
    ///     Updates an existing image collection with new images (updating URLs if index exists, adding new ones if not).
    /// </summary>
    private static List<Image> UpdateImageCollection(List<Image> existingImages,
        List<ClinicServiceEvent.Image> newImages)
    {
        // Convert existing images and new images into dictionaries for fast lookup (O(1) access)
        var newImageDict = newImages.ToDictionary(x => x.Index);
        var imageDict = existingImages.ToDictionary(x => x.Index);

        List<Image> newImagesReturn = new();

        // ✅ Handle images that exist in existingImages
        foreach (var key in imageDict.Keys)
            if (!newImageDict.TryGetValue(key, out _))
                // 🚀 Image exists in existingImages but not in newImages (KEEP IT)
                newImagesReturn.Add(new Image
                {
                    Id = imageDict[key].Id,
                    Index = imageDict[key].Index,
                    Url = imageDict[key].Url
                });
            else if (newImageDict.TryGetValue(key, out var image))
                // 🚀 Update existing image URL
                newImagesReturn.Add(new Image
                {
                    Id = image.Id,
                    Index = image.Index,
                    Url = image.Url
                });

        // ✅ Handle new images that exist in `newImages` but not in `existingImages`
        foreach (var key in newImageDict.Keys)
            if (!imageDict.ContainsKey(key))
                // 🚀 New image with a new index (ADD IT)
                newImagesReturn.Add(new Image
                {
                    Id = newImageDict[key].Id,
                    Index = newImageDict[key].Index,
                    Url = newImageDict[key].Url
                });

        return newImagesReturn;
    }
}