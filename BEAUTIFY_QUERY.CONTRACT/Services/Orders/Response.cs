namespace BEAUTIFY_QUERY.CONTRACT.Services.Orders;
public static class Response
{
    public record Order(
        Guid Id,
        string CustomerName,
        string ServiceName,
        decimal? TotalAmount,
        decimal? Discount,
        decimal? FinalAmount,
        DateTimeOffset OrderDate,
        string Status);
}