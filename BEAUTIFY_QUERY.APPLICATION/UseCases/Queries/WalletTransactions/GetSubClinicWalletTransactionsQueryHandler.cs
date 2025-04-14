using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WalletTransactions;
internal sealed class GetSubClinicWalletTransactionsQueryHandler(
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetSubClinicWalletTransactions, PagedResult<Response.WalletTransactionResponse>>
{
    public async Task<Result<PagedResult<Response.WalletTransactionResponse>>> Handle(
        Query.GetSubClinicWalletTransactions request,
        CancellationToken cancellationToken)
    {
        // Verify the current user is from the parent clinic
        var currentClinicId = currentUserService.ClinicId;
        if (currentClinicId == null || currentClinicId == Guid.Empty || currentClinicId != request.ParentClinicId)
        {
            return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                new Error("403", "Unauthorized access. You can only view transactions for your own sub-clinics."));
        }

        // Verify the clinic exists and is a parent clinic
        var parentClinic = await clinicRepository.FindByIdAsync(request.ParentClinicId, cancellationToken, x => x.Children);
        if (parentClinic == null)
        {
            return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                new Error("404", "Parent clinic not found."));
        }

        if (parentClinic.IsParent != true || !parentClinic.Children.Any())
        {
            return Result.Failure<PagedResult<Response.WalletTransactionResponse>>(
                new Error("400", "The specified clinic is not a parent clinic or has no sub-clinics."));
        }

        // Get all sub-clinic IDs
        var subClinicIds = parentClinic.Children.Select(c => c.Id).ToList();

        // Build the query to get transactions for all sub-clinics
        var query = walletTransactionRepository.FindAll(x => subClinicIds.Contains(x.ClinicId.Value) && !x.IsDeleted);

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
        Query.GetSubClinicWalletTransactions request)
    {
        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(x => 
                (x.Description != null && x.Description.ToLower().Contains(searchTerm)) ||
                x.Amount.ToString().Contains(searchTerm) ||
                (x.TransactionType != null && x.TransactionType.ToLower().Contains(searchTerm)) ||
                (x.Status != null && x.Status.ToLower().Contains(searchTerm)) ||
                (x.Clinic != null && x.Clinic.Name.ToLower().Contains(searchTerm)));
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
        Query.GetSubClinicWalletTransactions request)
    {
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            // Default sorting by transaction date descending
            return query.OrderByDescending(x => x.TransactionDate);
        }

        var sortOrder = request.SortOrder ?? BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Descending;

        return request.SortColumn.ToLower() switch
        {
            "clinicname" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.Clinic.Name)
                : query.OrderByDescending(x => x.Clinic.Name),
            "amount" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.Amount)
                : query.OrderByDescending(x => x.Amount),
            "transactiontype" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.TransactionType)
                : query.OrderByDescending(x => x.TransactionType),
            "status" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
                ? query.OrderBy(x => x.Status)
                : query.OrderByDescending(x => x.Status),
            "transactiondate" => sortOrder == BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations.SortOrder.Ascending
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
