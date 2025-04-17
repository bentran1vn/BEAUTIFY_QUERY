using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetClinicBranchesQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetClinicBranchesQuery, Response.GetClinicBranchesResponse>
{
    public async Task<Result<Response.GetClinicBranchesResponse>> Handle(
        Query.GetClinicBranchesQuery request,
        CancellationToken cancellationToken)
    {
        // Get the current user's clinic ID


        // Find the parent clinic associated with the current user
        var parentClinic = await clinicRepository
            .FindAll(c => c.UserClinics != null &&
                          c.UserClinics.Any(uc => uc.UserId == currentUserService.UserId.Value) &&
                          c.IsParent == true)
            .FirstOrDefaultAsync(cancellationToken);

        if (parentClinic == null)
            return Result.Failure<Response.GetClinicBranchesResponse>(
                new Error("404", "Parent clinic not found"));

        // Get all branches for this parent clinic
        var branches = await clinicRepository
            .FindAll(c => c.ParentId == parentClinic.Id && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        // Add the parent clinic to the list
        branches.Add(parentClinic);

        // Get all clinic IDs including parent and branches
        var clinicIds = branches.Select(c => c.Id).ToList();

        // Get pending withdrawals for all clinics
        var pendingWithdrawals = await walletTransactionRepository
            .FindAll(wt => clinicIds.Contains(wt.ClinicId.Value) &&
                           wt.TransactionType == Constant.WalletConstants.TransactionType.WITHDRAWAL &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.PENDING)
            .GroupBy(wt => wt.ClinicId)
            .Select(g => new { ClinicId = g.Key, PendingAmount = g.Sum(wt => wt.Amount) })
            .ToDictionaryAsync(x => x.ClinicId, x => x.PendingAmount, cancellationToken);

        // Get total earnings for all clinics
        var totalEarnings = await walletTransactionRepository
            .FindAll(wt => clinicIds.Contains(wt.ClinicId.Value) &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.COMPLETED)
            .GroupBy(wt => wt.ClinicId)
            .Select(g => new { ClinicId = g.Key, TotalAmount = g.Sum(wt => wt.Amount) })
            .ToDictionaryAsync(x => x.ClinicId, x => x.TotalAmount, cancellationToken);

        // Create response
        var response = new Response.GetClinicBranchesResponse
        {
            Clinics = branches.Select(clinic => new Response.ClinicBranchDto
            {
                Id = clinic.Id,
                Name = clinic.Name,
                Logo = clinic.ProfilePictureUrl,
                Balance = clinic.Balance,
                WorkingTimeEnd = clinic.WorkingTimeStart,
                WorkingTimeStart = clinic.WorkingTimeEnd,
                PendingWithdrawals =
                    pendingWithdrawals.TryGetValue(clinic.Id, out var pendingAmount) ? pendingAmount : 0,
                TotalEarnings = totalEarnings.TryGetValue(clinic.Id, out var earnings) ? earnings : 0,
                BankName = clinic.BankName,
                BankAccountNumber = clinic.BankAccountNumber,
                IsMainClinic = clinic.IsParent
            }).ToList(),
            Totals = new Response.TotalSummaryDto
            {
                TotalBalance = branches.Sum(c => c.Balance),
                TotalPendingWithdrawals = pendingWithdrawals.Values.Sum(),
                TotalEarnings = totalEarnings.Values.Sum()
            }
        };

        return Result.Success(response);
    }
}