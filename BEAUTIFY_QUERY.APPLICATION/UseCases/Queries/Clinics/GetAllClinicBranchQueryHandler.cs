using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetAllClinicBranchQueryHandler(IRepositoryBase<Clinic, Guid> _clinicRepository)
    : IQueryHandler<Query.GetAllClinicBranchQuery, PagedResult<Response.GetClinics>>
{
    public async Task<Result<PagedResult<Response.GetClinics>>> Handle(Query.GetAllClinicBranchQuery request,
        CancellationToken cancellationToken)
    {
        /* var clinic = await _clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken);
         if (clinic == null) return Result.Failure<List<Response.GetClinics>>(new Error("404", "Clinic not found"));
         var clinicBranches =
             await _clinicRepository.FindAll(x => x.ParentId == clinic.Id).ToListAsync(cancellationToken);
         var result = clinicBranches.Select(x =>
             new Response.GetClinics
             (x.Id,
                 x.Name, x.Email, x.Address, clinic.TotalBranches ?? 0, x.IsActivated)).ToList();

         return Result.Success(result);*/

        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        var query = _clinicRepository.FindAll();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.Email.Contains(searchTerm) ||
                x.Address.Contains(searchTerm));
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        var result = await PagedResult<Clinic>.CreateAsync(query, request.PageIndex, request.PageSize);

        var mapped = result.Items.Select(x =>
            new Response.GetClinics(x.Id, x.Name, x.Email, x.Address,
                x.TotalBranches ?? 0, x.IsActivated)).ToList();

        return Result.Success(
            new PagedResult<Response.GetClinics>(mapped, result.TotalCount, result.PageIndex, result.PageSize));
    }

    private static Expression<Func<Clinic, object>> GetSortProperty(Query.GetAllClinicBranchQuery request)
    {
        return request.SortColumn switch
        {
            "Name" => x => x.Name,
            "Address" => x => x.Address,
            "Email" => x => x.Email,
            _ => x => x.CreatedOnUtc
        };
    }
}