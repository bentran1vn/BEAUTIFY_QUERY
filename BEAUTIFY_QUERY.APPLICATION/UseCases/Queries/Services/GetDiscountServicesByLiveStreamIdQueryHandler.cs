using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using Microsoft.EntityFrameworkCore;
using Promotion = BEAUTIFY_QUERY.DOMAIN.Entities.Promotion;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;

public class GetDiscountServicesByLiveStreamIdQueryHandler : IQueryHandler<Query.GetDiscountServicesByLiveStreamIdQuery, Response.GetDiscountServicesByLiveStreamIdResponse>
{
    private readonly IRepositoryBase<Promotion , Guid> _promotionRepository;
    private readonly IRepositoryBase<Service , Guid> _serviceRepository;
    private readonly IRepositoryBase<LivestreamRoom , Guid> _livestreamRoomRepository;

    public GetDiscountServicesByLiveStreamIdQueryHandler(IRepositoryBase<Promotion, Guid> promotionRepository, IRepositoryBase<Service, Guid> serviceRepository, IRepositoryBase<LivestreamRoom, Guid> livestreamRoomRepository)
    {
        _promotionRepository = promotionRepository;
        _serviceRepository = serviceRepository;
        _livestreamRoomRepository = livestreamRoomRepository;
    }

    public async Task<Result<Response.GetDiscountServicesByLiveStreamIdResponse>> Handle(Query.GetDiscountServicesByLiveStreamIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.FindByIdAsync(request.ServiceId, cancellationToken); 

        if (service is null)
        {
            return Result.Failure<Response.GetDiscountServicesByLiveStreamIdResponse>(new Error("404", "Service not found"));
        }
        
        var promotion = await _promotionRepository
            .FindAll(x => 
                x.LivestreamRoomId == request.LiveStreamId &&
                x.ServiceId == request.ServiceId &&
                x.IsActivated)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (promotion is null)
        {
            return Result.Failure<Response.GetDiscountServicesByLiveStreamIdResponse>(new Error("404", "Promotion not found"));
        }
        
        var liveStream = await _livestreamRoomRepository.FindByIdAsync(request.LiveStreamId, cancellationToken); 

        if (liveStream is null)
        {
            return Result.Failure<Response.GetDiscountServicesByLiveStreamIdResponse>(new Error("404", "Service not found"));
        }
        
        var dateTime = liveStream.Date!.Value.ToDateTime(liveStream.StartDate!.Value);

        var dateResult = new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime));
        
        var response = new Response.GetDiscountServicesByLiveStreamIdResponse(
            request.ServiceId,
            request.LiveStreamId,
            liveStream.Name,
            dateResult,
            promotion.DiscountPercent
        );

        return Result.Success(response);
    }
}