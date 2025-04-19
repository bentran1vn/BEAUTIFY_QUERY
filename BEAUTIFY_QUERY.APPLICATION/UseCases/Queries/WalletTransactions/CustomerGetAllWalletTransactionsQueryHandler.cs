using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WalletTransactions;
internal sealed class CustomerGetAllWalletTransactionsQueryHandler(
    ICurrentUserService currentUserService,
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository)
    : IQueryHandler<Query.CustomerGetAllWalletTransactions, PagedResult<Response.WalletTransactionResponse>>
{
    private static readonly Func<WalletTransaction, Response.WalletTransactionResponse> _mapFunction =
        transaction => new Response.WalletTransactionResponse(
            transaction.Id,
            transaction.ClinicId,
            transaction.Clinic?.Name,
            transaction.Amount,
            transaction.TransactionType,
            transaction.Status,
            transaction.IsMakeBySystem,
            "",
            transaction.Description ?? string.Empty,
            transaction.TransactionDate,
            transaction.CreatedOnUtc);

    public async Task<Result<PagedResult<Response.WalletTransactionResponse>>> Handle(
        Query.CustomerGetAllWalletTransactions request, CancellationToken cancellationToken)
    {
        // Build the base query to get all transactions for the current user
        var query = walletTransactionRepository
            .FindAll(x => x.UserId == currentUserService.UserId && !x.IsDeleted);

        // Apply filters and sorting
        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        // Execute the query with pagination
        var transactions = await PagedResult<WalletTransaction>.CreateAsync(
            query.Include(x => x.User),
            request.PageIndex,
            request.PageSize);

        // Map results to response objects
        var mappedTransactions = transactions.Items.Select(_mapFunction).ToList();

        return Result.Success(
            new PagedResult<Response.WalletTransactionResponse>(
                mappedTransactions,
                transactions.PageIndex,
                transactions.PageSize,
                transactions.TotalCount));
    }

    private static IQueryable<WalletTransaction> ApplyFilters(
        IQueryable<WalletTransaction> query,
        Query.CustomerGetAllWalletTransactions request)
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
            (x.Status != null && EF.Functions.Like(x.Status.ToLower(), $"%{searchTerm}%")) ||
            (x.Clinic != null && x.Clinic.Name != null &&
             EF.Functions.Like(x.Clinic.Name.ToLower(), $"%{searchTerm}%")));
    }

    private static IQueryable<WalletTransaction> ApplySorting(
        IQueryable<WalletTransaction> query,
        Query.CustomerGetAllWalletTransactions request)
    {
        // Get sort column (or default) and sort order (or default)
        var sortColumn = request.SortColumn?.ToLower() ?? "transactiondate";
        var sortOrder = request.SortOrder ??
                        SortOrder.Descending;
        var isAscending = sortOrder == SortOrder.Ascending;

        // Apply sorting using a switch expression
        return sortColumn switch
        {
            "clinicname" => isAscending
                ? query.OrderBy(x => x.Clinic != null ? x.Clinic.Name : string.Empty)
                : query.OrderByDescending(x => x.Clinic != null ? x.Clinic.Name : string.Empty),

            "amount" => isAscending
                ? query.OrderBy(x => x.Amount)
                : query.OrderByDescending(x => x.Amount),

            "transactiontype" => isAscending
                ? query.OrderBy(x => x.TransactionType ?? string.Empty)
                : query.OrderByDescending(x => x.TransactionType ?? string.Empty),

            "status" => isAscending
                ? query.OrderBy(x => x.Status ?? string.Empty)
                : query.OrderByDescending(x => x.Status ?? string.Empty),

            "transactiondate" => isAscending
                ? query.OrderBy(x => x.TransactionDate)
                : query.OrderByDescending(x => x.TransactionDate),

            "createdonutc" => isAscending
                ? query.OrderBy(x => x.CreatedOnUtc)
                : query.OrderByDescending(x => x.CreatedOnUtc),

            // Default to transaction date descending
            _ => query.OrderByDescending(x => x.TransactionDate)
        };
    }
}