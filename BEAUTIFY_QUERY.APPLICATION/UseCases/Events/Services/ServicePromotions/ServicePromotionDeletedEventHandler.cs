using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ServicePromotion;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServicePromotions;
public class ServicePromotionDeletedEventHandler : ICommandHandler<DomainEvents.ServicePromotionDeleted>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ServicePromotionDeletedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ServicePromotionDeleted request, CancellationToken cancellationToken)
    {
        var deleteRequest = request.entity;

        var isServiceExisted = await _clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(deleteRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {deleteRequest.ServiceId} not found");

        var promotions = isServiceExisted.Promotions?.ToList() ?? new List<Promotion>();

        var promotionToUpdate = promotions.FirstOrDefault(x => x.Id == deleteRequest.PromotionId);

        if (promotionToUpdate == null)
            throw new Exception($"Promotion {deleteRequest.PromotionId} not found");

        promotions.Remove(promotionToUpdate);

        isServiceExisted.DiscountPercent = 0;
        isServiceExisted.DiscountMaxPrice = isServiceExisted.MaxPrice;
        isServiceExisted.DiscountMinPrice = isServiceExisted.MinPrice;
        isServiceExisted.Promotions = promotions;

        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}