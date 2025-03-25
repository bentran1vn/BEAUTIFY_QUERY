using System.Globalization;
using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
public class GetClinicDetailQueryHandler(
    IRepositoryBase<Clinic, Guid> clinicRepository,
    ICurrentUserService currentUserService,
    IMongoRepository<ClinicServiceProjection> _clinicServiceRepository)
    : IQueryHandler<Query.GetClinicDetailQuery, Response.GetClinicDetail>
{
    public async Task<Result<Response.GetClinicDetail>> Handle(Query.GetClinicDetailQuery request,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepository.FindByIdAsync(request.id, cancellationToken);
        if (clinic == null || clinic.IsDeleted)
            return Result.Failure<Response.GetClinicDetail>(new Error("404", "Clinic not found."));
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        var query = currentUserService.Role == Constant.Role.CLINIC_ADMIN
            ? clinicRepository.FindAll(x => x.ParentId == request.id)
            : clinicRepository.FindAll(x => x.ParentId == null);
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm) ||
                                     x.City.Contains(searchTerm) ||
                                     x.Address.Contains(searchTerm) ||
                                     x.Ward.Contains(searchTerm) ||
                                     x.District.Contains(searchTerm) ||
                                     x.PhoneNumber.Contains(searchTerm));
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        var branches = await PagedResult<Clinic>.CreateAsync(query, request.PageIndex, request.PageSize);
        var branchDetails = new PagedResult<Response.GetClinicDetail>(
            branches.Items.Select(x => MapToResponse(x)).ToList(),
            branches.TotalCount,
            branches.PageIndex,
            branches.PageSize
        );
        var services = _clinicServiceRepository.FilterBy(x =>
            x.Clinic.Any(clinic => clinic.Id.Equals(request.id))
        ).ToList();

        var mappedServices = services.Select(x => new CONTRACT.Services.Services.Response.GetAllServiceInGetClinicById
        {
            Id = x.DocumentId,
            Name = x.Name,
            Description = x.Description,
            MaxPrice = x.MaxPrice,
            MinPrice = x.MinPrice,
            DiscountPercent = (x.DiscountPercent * 100).ToString(CultureInfo.InvariantCulture),
            DiscountMaxPrice = x.DiscountMaxPrice,
            DiscountMinPrice = x.DiscountMinPrice,
            CoverImage = x.CoverImage.Select(x => new CONTRACT.Services.Services.Response.Image(x.Id, x.Index, x.Url))
                .ToList(),
            DescriptionImage = x.DescriptionImage
                .Select(x => new CONTRACT.Services.Services.Response.Image(x.Id, x.Index, x.Url)).ToList()
        }).ToList();

        var result = MapToResponse(clinic, branchDetails, mappedServices);
        return Result.Success(result);
    }

    private static Response.GetClinicDetail MapToResponse(Clinic? clinic,
        PagedResult<Response.GetClinicDetail>? branches = null,
        List<CONTRACT.Services.Services.Response.GetAllServiceInGetClinicById>? services = null)
    {
        return new Response.GetClinicDetail(
            clinic.Id,
            clinic.Name,
            clinic.Email,
            clinic.PhoneNumber,
            clinic.City,
            clinic.Address,
            clinic.District,
            clinic.Ward,
            clinic.FullAddress,
            clinic.TaxCode,
            clinic.BusinessLicenseUrl,
            clinic.OperatingLicenseUrl,
            clinic.OperatingLicenseExpiryDate,
            clinic.ProfilePictureUrl,
            clinic.TotalBranches ?? 0,
            clinic.IsActivated,
            clinic.BankName,
            clinic.BankAccountNumber,
            branches,
            services);
    }

    private static Expression<Func<Clinic, object>> GetSortProperty(Query.GetClinicDetailQuery request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => x => x.Name,
            _ => x => x.CreatedOnUtc
        };
    }
}