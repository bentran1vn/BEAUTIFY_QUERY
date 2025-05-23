﻿namespace BEAUTIFY_QUERY.CONTRACT.Services.OrderDetails;
public static class Response
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public string? ProcedureName { get; set; }
        public string? ProcedurePriceType { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string? StepIndex { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
    }
}