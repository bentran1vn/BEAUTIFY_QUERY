namespace BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
public static class Response
{
    public record WalletTransactionResponse(
        Guid Id,
        Guid? ClinicId,
        string? ClinicName,
        decimal Amount,
        string TransactionType,
        string Status,
        bool IsMakeBySystem,
        string? NewestQrUrl,
        string? Description,
        DateTime TransactionDate,
        DateTimeOffset CreatedOnUtc);

    public record ClinicBasicInfo(
        Guid Id,
        string Name,
        string Email,
        string? PhoneNumber,
        string? Address,
        bool IsParent,
        bool IsActivated);
}