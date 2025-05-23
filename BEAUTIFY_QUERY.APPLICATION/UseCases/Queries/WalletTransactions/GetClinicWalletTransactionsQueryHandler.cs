using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WalletTransactions;
internal sealed class GetClinicWalletTransactionsQueryHandler(
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetClinicWalletTransactions, PagedResult<Response.WalletTransactionResponse>>
{
    public async Task<Result<PagedResult<Response.WalletTransactionResponse>>> Handle(
        Query.GetClinicWalletTransactions request,
        CancellationToken cancellationToken)
    {
        // Get the current clinic ID from the authenticated user
        var clinicId = currentUserService.ClinicId;

        // Build the query
        var query = walletTransactionRepository.FindAll(x => x.ClinicId == clinicId && !x.IsDeleted);

        // Apply filters
        query = ApplyFilters(query, request);

        // Apply sorting
        query = ApplySorting(query, request);

        // Execute the query with pagination
        var transactions = await PagedResult<WalletTransaction>.CreateAsync(
            query.Include(x => x.Clinic),
            request.PageIndex,
            request.PageSize);

        // Map to response
        var mappedTransactions = transactions.Items.Select(MapToResponse).ToList();

        return Result.Success(
            new PagedResult<Response.WalletTransactionResponse>(
                mappedTransactions,
                transactions.PageIndex,
                transactions.PageSize,
                transactions.TotalCount));
    }

    private static IQueryable<WalletTransaction> ApplyFilters(
        IQueryable<WalletTransaction> query,
        Query.GetClinicWalletTransactions request)
    {
        var searchTerm = request.SearchTerm?.Trim().ToLower();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Check if search term contains "to" for date or amount range
            if (searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            {
                var parts = searchTerm.Split("to", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var part1 = parts[0].Trim();
                    var part2 = parts[1].Trim();

                    // Try to parse as a date range
                    if (DateTimeOffset.TryParse(part1, out var dateFrom) &&
                        DateTimeOffset.TryParse(part2, out var dateTo))
                    {
                        // Normalize dateTo to end of day
                        dateTo = dateTo.Date.AddDays(1).AddTicks(-1);
                        query = query.Where(x => x.TransactionDate >= dateFrom && x.TransactionDate <= dateTo);
                    }
                    // Try to parse as an amount range
                    else if (decimal.TryParse(part1, out var amountFrom) &&
                             decimal.TryParse(part2, out var amountTo))
                    {
                        query = query.Where(x => x.Amount >= amountFrom && x.Amount <= amountTo);
                    }
                    else
                    {
                        // If the range parts can't be parsed, fall back to a standard contains search
                        query = ApplyStandardSearch(query, searchTerm);
                    }
                }
                else
                {
                    // If "to" is present but splitting doesn't yield exactly two parts,
                    // use the standard search
                    query = ApplyStandardSearch(query, searchTerm);
                }
            }
            // Check for transaction types
            else if (searchTerm.Equals("deposit", StringComparison.OrdinalIgnoreCase) ||
                     searchTerm.Equals("withdrawal", StringComparison.OrdinalIgnoreCase) ||
                     searchTerm.Equals("transfer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.TransactionType != null &&
                                         x.TransactionType.ToLower() == searchTerm);
            }
            // Check for status types
            else if (searchTerm.Equals("pending", StringComparison.OrdinalIgnoreCase) ||
                     searchTerm.Equals("completed", StringComparison.OrdinalIgnoreCase) ||
                     searchTerm.Equals("failed", StringComparison.OrdinalIgnoreCase) ||
                     searchTerm.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status != null &&
                                         x.Status.ToLower() == searchTerm);
            }
            // Check for date
            else if (DateTimeOffset.TryParse(searchTerm, out var singleDate))
            {
                var endOfDay = singleDate.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.TransactionDate >= singleDate.Date &&
                                         x.TransactionDate <= endOfDay);
            }
            // Standard search for all other cases
            else
            {
                query = ApplyStandardSearch(query, searchTerm);
            }
        }

        return query;
    }

    private static IQueryable<WalletTransaction> ApplyStandardSearch(
        IQueryable<WalletTransaction> query,
        string searchTerm)
    {
        return query.Where(x =>
            (x.Description != null && EF.Functions.Like(x.Description.ToLower(), $"%{searchTerm}%")) ||
            EF.Functions.Like(x.Amount.ToString(), $"%{searchTerm}%") ||
            (x.TransactionType != null && EF.Functions.Like(x.TransactionType.ToLower(), $"%{searchTerm}%")) ||
            (x.Status != null && EF.Functions.Like(x.Status.ToLower(), $"%{searchTerm}%")));
    }

    private static IQueryable<WalletTransaction> ApplySorting(
        IQueryable<WalletTransaction> query,
        Query.GetClinicWalletTransactions request)
    {
        if (string.IsNullOrWhiteSpace(request.SortColumn))
            // Default sorting by transaction date descending
            return query.OrderByDescending(x => x.TransactionDate);

        var sortOrder = request.SortOrder ?? SortOrder.Descending;

        return request.SortColumn.ToLower() switch
        {
            "amount" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(x => x.Amount)
                : query.OrderByDescending(x => x.Amount),
            "transactiontype" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(x => x.TransactionType)
                : query.OrderByDescending(x => x.TransactionType),
            "status" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(x => x.Status)
                : query.OrderByDescending(x => x.Status),
            "transactiondate" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(x => x.TransactionDate)
                : query.OrderByDescending(x => x.TransactionDate),
            "createdonutc" => sortOrder == SortOrder.Ascending
                ? query.OrderBy(x => x.CreatedOnUtc)
                : query.OrderByDescending(x => x.CreatedOnUtc),
            _ => query.OrderByDescending(x => x.TransactionDate)
        };
    }

    private static Response.WalletTransactionResponse MapToResponse(WalletTransaction transaction)
    {
        return new Response.WalletTransactionResponse(
            transaction.Id,
            transaction.ClinicId,
            transaction.Clinic?.Name,
            transaction.Amount,
            transaction.ProofImageUrl,
            transaction.TransactionType,
            transaction.Status,
            transaction.IsMakeBySystem,
            "",
            transaction.Description,
            "",
            "",
            transaction.TransactionDate,
            transaction.CreatedOnUtc);
    }
}