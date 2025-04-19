using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using Microsoft.EntityFrameworkCore;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetClinicByIdQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository)
    : IQueryHandler<Query.GetClinicByIdQuery, Response.ClinicBranchDto>
{
    public async Task<Result<Response.ClinicBranchDto>> Handle(
        Query.GetClinicByIdQuery request,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepository.FindByIdAsync(request.Id, cancellationToken);

        if (clinic == null || clinic.IsDeleted)
            return Result.Failure<Response.ClinicBranchDto>(new Error("404", "Clinic not found"));

        // Get pending withdrawals for the clinic
        var pendingWithdrawals = await walletTransactionRepository
            .FindAll(wt => wt.ClinicId == clinic.Id &&
                           wt.TransactionType == Constant.WalletConstants.TransactionType.WITHDRAWAL &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.PENDING)
            .SumAsync(wt => wt.Amount, cancellationToken);

        // Get total earnings for the clinic
        var totalEarnings = await walletTransactionRepository
            .FindAll(wt => wt.ClinicId == clinic.Id &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.COMPLETED)
            .SumAsync(wt => wt.Amount, cancellationToken);

        return Result.Success(new Response.ClinicBranchDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            Logo = clinic.ProfilePictureUrl,
            Balance = clinic.Balance,
            PendingWithdrawals = pendingWithdrawals,
            WorkingTimeEnd = clinic.WorkingTimeStart,
            WorkingTimeStart = clinic.WorkingTimeEnd,
            TotalEarnings = totalEarnings,
            BankName = clinic.BankName ?? string.Empty,
            BankAccountNumber = clinic.BankAccountNumber ?? string.Empty,
            IsMainClinic = clinic.IsParent
        });
    }
}