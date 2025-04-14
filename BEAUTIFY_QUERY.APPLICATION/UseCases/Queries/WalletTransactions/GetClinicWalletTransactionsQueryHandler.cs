using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
using BEAUTIFY_QUERY.DOMAIN.Entities;
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
        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                (x.Description != null && x.Description.ToLower().Contains(searchTerm)) ||
                x.Amount.ToString().Contains(searchTerm) ||
                (x.TransactionType != null && x.TransactionType.ToLower().Contains(searchTerm)) ||
                (x.Status != null && x.Status.ToLower().Contains(searchTerm)));
        }

        // Apply transaction type filter
        if (!string.IsNullOrWhiteSpace(request.TransactionType))
        {
            query = query.Where(x => x.TransactionType == request.TransactionType);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate <= request.EndDate.Value);
        }

        return query;
    }

    private static IQueryable<WalletTransaction> ApplySorting(
        IQueryable<WalletTransaction> query,
        Query.GetClinicWalletTransactions request)
    {
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            // Default sorting by transaction date descending
            return query.OrderByDescending(x => x.TransactionDate);
        }

        var sortOrder = request.SortOrder ??
                        BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Descending;

        return request.SortColumn.ToLower() switch
        {
            "amount" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.Amount)
                : query.OrderByDescending(x => x.Amount),
            "transactiontype" => sortOrder ==
                                 BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.TransactionType)
                : query.OrderByDescending(x => x.TransactionType),
            "status" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.Status)
                : query.OrderByDescending(x => x.Status),
            "transactiondate" => sortOrder ==
                                 BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.TransactionDate)
                : query.OrderByDescending(x => x.TransactionDate),
            "createdonutc" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
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
            transaction.TransactionType,
            transaction.Status,
            transaction.IsMakeBySystem,
            transaction.Description,
            transaction.TransactionDate,
            transaction.CreatedOnUtc);
    }
}