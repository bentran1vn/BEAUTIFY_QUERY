using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;

namespace BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
public static class Query
{
    /// <summary>
    ///     Query to get wallet transactions for the current clinic
    /// </summary>
    public record GetClinicWalletTransactions(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.WalletTransactionResponse>>;

    /// <summary>
    ///     Query to get wallet transactions for all sub-clinics of a parent clinic
    /// </summary>
    public record GetSubClinicWalletTransactions(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.WalletTransactionResponse>>;

    /// <summary>
    ///     Query to get wallet transactions for all clinics (admin only)
    /// </summary>
    public record GetAllClinicWalletTransactions(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.WalletTransactionResponse>>;

    /// <summary>
    ///     Query to get wallet transactions for a specific clinic by ID
    /// </summary>
    public record GetWalletHistoryByClinicId(
        Guid ClinicId,
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.WalletTransactionResponse>>;

    public record CustomerGetAllWalletTransactions(
        string? SearchTerm,
        string? SortColumn,
        SortOrder? SortOrder,
        int PageIndex,
        int PageSize) : IQuery<PagedResult<Response.WalletTransactionResponse>>;
}