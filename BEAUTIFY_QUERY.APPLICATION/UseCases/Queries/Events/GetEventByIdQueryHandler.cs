using BEAUTIFY_QUERY.CONTRACT.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Events;

public class GetEventByIdQueryHandler : IQueryHandler<Query.GetEventByIdQuery, Response.EventDetailResponse>
{
    private readonly IRepositoryBase<Event, Guid> _eventRepository;

    public GetEventByIdQueryHandler(IRepositoryBase<Event, Guid> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<Response.EventDetailResponse>> Handle(Query.GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _eventRepository.FindAll(x => !x.IsDeleted && x.Id == request.Id);
        
        if(request.ClinicId != null)
        {
            
            query = query.Include(x => x.LivestreamRoom ?? new List<LivestreamRoom>())
                .ThenInclude(x => x.LiveStreamDetail);
        }
        else
        {
            query = query
                .Include(x => x.LivestreamRoom ?? new List<LivestreamRoom>());
        }
        
        var eventDetail = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (eventDetail == null)
        {
            return Result.Failure<Response.EventDetailResponse>(new Error("404", "Event not found"));
        }

        List<Response.LivestreamRoomResponse> livestreamRooms = eventDetail.LivestreamRoom?.Select(
            x => new Response.LivestreamRoomResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description ?? "",
                Image = x.Image ?? "",
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status?.ToString(),
                Date = x.Date,
                Type = x.Type?.ToString(),
                LiveStreamDetail = request.ClinicId != null ? new Response.LiveStreamDetail()
                {
                    JoinCount = x.LiveStreamDetail?.JoinCount ?? 0,
                    MessageCount = x.LiveStreamDetail?.MessageCount ?? 0,
                    ReactionCount = x.LiveStreamDetail?.ReactionCount ?? 0,
                    TotalActivities = x.LiveStreamDetail?.TotalActivities ?? 0,
                    TotalBooking = x.LiveStreamDetail?.TotalBooking ?? 0,
                    CreatedOnUtc = x.LiveStreamDetail?.CreatedOnUtc ?? DateTimeOffset.MinValue
                } : null
            }).ToList() ?? new();

        var result = new Response.EventDetailResponse()
        {
            Id = eventDetail.Id,
            Name = eventDetail.Name,
            Description = eventDetail.Description ?? "",
            StartDate = eventDetail.StartDate ?? DateTimeOffset.MinValue,
            EndDate = eventDetail.EndDate ?? DateTimeOffset.MinValue,
            ImageUrl = eventDetail.Image ?? "",
            ClinicName = eventDetail.Clinic?.Name ?? "",
            ClinicId = eventDetail.ClinicId ?? Guid.Empty,
            LivestreamRooms = livestreamRooms
        };
        
        return Result.Success(result);
    }
}