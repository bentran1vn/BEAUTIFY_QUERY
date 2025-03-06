namespace BEAUTIFY_QUERY.CONTRACT.Services.OrderDetails;
public static class Response
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public string? ServiceName { get; set; }
        public string? ProcedurePriceType { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
    }
}