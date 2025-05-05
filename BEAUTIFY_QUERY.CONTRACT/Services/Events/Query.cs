namespace BEAUTIFY_QUERY.CONTRACT.Services.Events;

public class Query
{
    public class GetEvent
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class GetEventQuery: GetEvent, IQuery<PagedResult<Response.EventResponse>>
    {
        public GetEventQuery(DateTimeOffset? startDate, DateTimeOffset? endDate, 
            string? searchTerm, int pageNumber, int pageSize)
        {
            StartDate = startDate;
            EndDate = endDate;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    public class GetClinicEventQuery: GetEvent, IQuery<PagedResult<Response.EventResponse>>
    {
        public Guid ClinicId { get; set; }

        public GetClinicEventQuery(DateTimeOffset? startDate, DateTimeOffset? endDate, string? searchTerm, int pageNumber, int pageSize,
            Guid clinicId)
        {
            StartDate = startDate;
            EndDate = endDate;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
            ClinicId = clinicId;
        }
    }
    
    public class GetEventByIdQuery: IQuery<Response.EventDetailResponse>
    {
        public Guid Id { get; set; }
        public Guid? ClinicId { get; set; }
    }
}