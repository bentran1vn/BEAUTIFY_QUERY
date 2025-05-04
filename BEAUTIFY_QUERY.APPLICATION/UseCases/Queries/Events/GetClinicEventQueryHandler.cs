using BEAUTIFY_QUERY.CONTRACT.Services.Events;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Events;

public class GetClinicEventQueryHandler : IQueryHandler<Query.GetClinicEventQuery, PagedResult<Response.EventResponse>>
{
    private readonly IRepositoryBase<Event, Guid> _eventRepository;

    public GetClinicEventQueryHandler(IRepositoryBase<Event, Guid> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<PagedResult<Response.EventResponse>>> Handle(Query.GetClinicEventQuery request, CancellationToken cancellationToken)
    {
        // Get the current date and time in Vietnam timezone
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var currentDateTimeVN = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnamTimeZone);

        var query = _eventRepository.FindAll(x => !x.IsDeleted);

        query = query.Include(x => x.Clinic);
        
        query = string.IsNullOrWhiteSpace(request.SearchTerm) 
            ? query
            : query.Where(x => x.Name.ToLower().Contains(request.SearchTerm.ToLower()));
        
        query = query.Where(x => x.ClinicId == request.ClinicId);
        
        // Apply time range filter if provided
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            query = query.Where(x => x.StartDate >= request.StartDate && x.EndDate <= request.EndDate);
        }
        
        // Sort events by how close they are to current time
        // First get future events (today and later)
        var futureEvents = query.Where(x => x.StartDate >= currentDateTimeVN);
            
        // Then get past events (before today or today but already started)
        var pastEvents = query.Where(x => x.StartDate < currentDateTimeVN);
            
        // Order future events by date and time (ascending - closest future events first)
        futureEvents = futureEvents.OrderBy(x => x.StartDate);
            
        // Order past events by date and time (descending - most recent past events first)
        pastEvents = pastEvents.OrderByDescending(x => x.StartDate);
            
        // Combine the queries - future events first, then past events
        query = futureEvents.Concat(pastEvents);
        
        var events = await PagedResult<Event>.CreateAsync(query, request.PageNumber, request.PageSize);
        
        var result = new PagedResult<Response.EventResponse>(events.Items
                .Select(x => new Response.EventResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description ?? "",
                    StartDate = x.StartDate ?? DateTimeOffset.MinValue,
                    EndDate = x.EndDate ?? DateTimeOffset.MinValue,
                    ClinicId = x.ClinicId ?? Guid.Empty,
                    ClinicName = x.Clinic?.Name ?? "",
                    ImageUrl = x.Clinic?.ProfilePictureUrl ?? "",
                })
                .ToList(),
            events.PageIndex, events.PageSize, events.TotalCount);

        return Result.Success(result);
    }
}