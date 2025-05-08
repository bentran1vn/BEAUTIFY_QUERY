using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
using BEAUTIFY_QUERY.DOMAIN;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WalletTransactions;
internal sealed class GetWalletHistoryByClinicIdQueryHandler(
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetWalletHistoryByClinicId, PagedResult<Response.WalletTransactionResponse>>
{
    public async Task<Result<PagedResult<Response.WalletTransactionResponse>>> Handle(
        Query.GetWalletHistoryByClinicId request,
        CancellationToken cancellationToken)
    {
        // Verify the current user has admin role
        if (currentUserService.Role != Constant.Role.CLINIC_ADMIN &&
            currentUserService.Role != Constant.Role.SYSTEM_ADMIN)

            // If the user is a clinic admin, verify they are requesting data for their own clinic or a sub-clinic
            if (currentUserService.Role == Constant.Role.CLINIC_ADMIN)
            {
                var adminClinicId = currentUserService.ClinicId;

                // If requesting data for a different clinic than their own
                if (request.ClinicId != adminClinicId)
                {
                    // Check if the requested clinic is a sub-clinic of the admin's clinic
                    var requestedClinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);

                    if (requestedClinic == null)
                        return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                            new Error("404", ErrorMessages.Clinic.ClinicNotFound));

                    if (requestedClinic.ParentId != adminClinicId)
                        return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                            new Error("403", "You can only access wallet history for your own clinic or its branches"));
                }
            }

        // Verify the clinic exists
        var clinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);
        if (clinic == null)
            return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                new Error("404", ErrorMessages.Clinic.ClinicNotFound));

        // Build the query to get transactions for the specified clinic
        var query = walletTransactionRepository.FindAll(x => x.ClinicId == request.ClinicId && !x.IsDeleted);

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
        Query.GetWalletHistoryByClinicId request)
    {
        var searchTerm = request.SearchTerm?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(searchTerm)) return query;

        // Check if search term contains "to" for date or amount range
        if (searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
        {
            var parts = searchTerm.Split("to", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return query;
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
        Query.GetWalletHistoryByClinicId request)
    {
        var sortColumn = request.SortColumn?.ToLower();
        var isAscending = request.SortOrder == SortOrder.Ascending;

        // Apply sorting using a switch expression
        return sortColumn switch
        {
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
            "", transaction.TransactionDate,
            transaction.CreatedOnUtc);
    }
}