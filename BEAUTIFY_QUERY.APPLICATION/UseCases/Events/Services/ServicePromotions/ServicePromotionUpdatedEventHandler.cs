using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ServicePromotion;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServicePromotions;
public class ServicePromotionUpdatedEventHandler : ICommandHandler<DomainEvents.ServicePromotionUpdated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ServicePromotionUpdatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ServicePromotionUpdated request, CancellationToken cancellationToken)
    {
        var updateRequest = request.entity;

        var isServiceExisted = await _clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(updateRequest.ServiceId));

        if (isServiceExisted == null) throw new Exception($"Service {updateRequest.ServiceId} not found");

        var promotions = isServiceExisted.Promotions?.ToList() ?? new List<Promotion>();

        var promotionToUpdate = promotions.FirstOrDefault(x => x.Id == updateRequest.PromotionId);

        if (promotionToUpdate == null)
            throw new Exception($"Promotion {updateRequest.PromotionId} not found");

        if (promotionToUpdate.IsActivated == false && updateRequest.IsActivated)
        {
            var lastestPromotion = promotions.FirstOrDefault(x => x.IsActivated);
            if (lastestPromotion != null) lastestPromotion.IsActivated = false;
            promotionToUpdate.IsActivated = true;
        }

        promotionToUpdate.Name = updateRequest.Name;
        promotionToUpdate.DiscountPercent = updateRequest.DiscountPercent;
        promotionToUpdate.ImageUrl = updateRequest.ImageUrl;
        promotionToUpdate.StartDay = DateTimeOffset.Parse(updateRequest.StartDay.ToString());
        promotionToUpdate.EndDate = DateTimeOffset.Parse(updateRequest.EndDate.ToString());
        promotionToUpdate.IsActivated = updateRequest.IsActivated;

        isServiceExisted.DiscountPercent = (decimal)updateRequest.DiscountPercent;
        isServiceExisted.DiscountMaxPrice = updateRequest.DiscountMaxPrice;
        isServiceExisted.DiscountMinPrice = updateRequest.DiscountMinPrice;
        isServiceExisted.Promotions = promotions;

        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}