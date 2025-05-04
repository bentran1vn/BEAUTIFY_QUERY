namespace BEAUTIFY_QUERY.CONTRACT.Services.Events;

public class Query
{
    public class GetEvent
    {
        public TimeOnly? StartDate { get; set; }
        public TimeOnly? EndDate { get; set; }
        public DateOnly? Date { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class GetEventQuery: GetEvent, IQuery<PagedResult<Response.EventResponse>>
    {
        public GetEventQuery(TimeOnly? startDate, TimeOnly? endDate,
            DateOnly? date, string? searchTerm, int pageNumber, int pageSize)
        {
            StartDate = startDate;
            EndDate = endDate;
            Date = date;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    public class GetClinicEventQuery: GetEvent, IQuery<PagedResult<Response.EventResponse>>
    {
        public Guid ClinicId { get; set; }

        public GetClinicEventQuery(TimeOnly? startDate, TimeOnly? endDate,
            DateOnly? date, string? searchTerm, int pageNumber, int pageSize,
            Guid clinicId)
        {
            StartDate = startDate;
            EndDate = endDate;
            Date = date;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
            ClinicId = clinicId;
        }
    }
}