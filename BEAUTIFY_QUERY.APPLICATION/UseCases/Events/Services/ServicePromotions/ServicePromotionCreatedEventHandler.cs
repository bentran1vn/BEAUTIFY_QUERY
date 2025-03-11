using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.ServicePromotion;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.ServicePromotions;

public class ServicePromotionCreatedEventHandler: ICommandHandler<DomainEvents.ServicePromotionCreated>
{
    private readonly IMongoRepository<ClinicServiceProjection> _clinicServiceRepository;

    public ServicePromotionCreatedEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    {
        _clinicServiceRepository = clinicServiceRepository;
    }

    public async Task<Result> Handle(DomainEvents.ServicePromotionCreated request, CancellationToken cancellationToken)
    {
        var createRequest = request.entity;

        var isServiceExisted = await _clinicServiceRepository
            .FindOneAsync(p => p.DocumentId.Equals(createRequest.ServiceId));
        
        if(isServiceExisted == null) throw new Exception($"Service {createRequest.ServiceId} not found");

        var promotion = new Promotion()
        {
            Id = createRequest.PromotionId,
            Name = createRequest.Name,
            DiscountPercent = createRequest.DiscountPercent,
            ImageUrl = createRequest.ImageUrl,
            StartDay = DateTimeOffset.Parse(createRequest.StartDay.ToString()),
            EndDate = DateTimeOffset.Parse(createRequest.EndDate.ToString()),
            IsActivated = true
        };
        
        var promotions = isServiceExisted.Promotions?.ToList() ?? new List<Promotion>();
        
        var lastestPromotion = promotions.FirstOrDefault( x => x.IsActivated );

        if (lastestPromotion != null) lastestPromotion.IsActivated = false;
        
        promotions.Add(promotion);
        
        isServiceExisted.Promotions = promotions;
        
        isServiceExisted.DiscountPercent = (decimal)createRequest.DiscountPercent;
        
        isServiceExisted.DiscountMaxPrice = createRequest.DiscountMaxPrice;
        isServiceExisted.DiscountMinPrice = createRequest.DiscountMinPrice;
        
        await _clinicServiceRepository.ReplaceOneAsync(isServiceExisted);
        
        return Result.Success();
    }
}