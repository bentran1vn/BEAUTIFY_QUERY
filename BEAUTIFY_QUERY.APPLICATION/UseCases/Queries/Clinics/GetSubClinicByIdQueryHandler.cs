using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetSubClinicByIdQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    IRepositoryBase<WalletTransaction, Guid> walletTransactionRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetSubClinicByIdQuery, Response.ClinicBranchDto>
{
    public async Task<Result<Response.ClinicBranchDto>> Handle(
        Query.GetSubClinicByIdQuery request, 
        CancellationToken cancellationToken)
    {
        // Get the current user's clinic ID
        var adminClinicId = currentUserService.ClinicId;
        
        // Find the requested clinic
        var requestedClinic = await clinicRepository.FindByIdAsync(request.Id, cancellationToken);
        
        if (requestedClinic == null || requestedClinic.IsDeleted)
            return Result.Failure<Response.ClinicBranchDto>(new Error("404", "Clinic not found"));
            
        // Check if the requested clinic is a child of the admin's clinic
        if (requestedClinic.ParentId != adminClinicId)
        {
            return Result.Failure<Response.ClinicBranchDto>(
                new Error("403", "You can only access clinics that are branches of your main clinic"));
        }

        // Get pending withdrawals for the clinic
        var pendingWithdrawals = await walletTransactionRepository
            .FindAll(wt => wt.ClinicId == requestedClinic.Id &&
                           wt.TransactionType == Constant.WalletConstants.TransactionType.WITHDRAWAL &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.PENDING)
            .SumAsync(wt => wt.Amount, cancellationToken);

        // Get total earnings for the clinic
        var totalEarnings = await walletTransactionRepository
            .FindAll(wt => wt.ClinicId == requestedClinic.Id &&
                           wt.Status == Constant.WalletConstants.TransactionStatus.COMPLETED)
            .SumAsync(wt => wt.Amount, cancellationToken);

        return Result.Success(new Response.ClinicBranchDto
        {
            Id = requestedClinic.Id,
            Name = requestedClinic.Name,
            Logo = requestedClinic.ProfilePictureUrl,
            Balance = requestedClinic.Balance,
            PendingWithdrawals = pendingWithdrawals,
            TotalEarnings = totalEarnings,
            BankName = requestedClinic.BankName ?? string.Empty,
            BankAccountNumber = requestedClinic.BankAccountNumber ?? string.Empty,
            IsMainClinic = requestedClinic.IsParent
        });
    }
}
