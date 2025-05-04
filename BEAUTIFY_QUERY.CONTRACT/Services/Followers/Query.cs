namespace BEAUTIFY_QUERY.CONTRACT.Services.Followers;

public class Query
{
    public class GetFollowerQuery : IQuery<PagedResult<Response.GetFollowerResponse>>
    {
        public Guid ClinicId { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}