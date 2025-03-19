using System.Globalization;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
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

        var branches = currentUserService.Role == Constant.Role.CLINIC_ADMIN
            ? await clinicRepository.FindAll(x => x.ParentId == request.id).ToListAsync(cancellationToken)
            : [];
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

        var branchDetails = branches.Select(x => MapToResponse(x)).ToList();

        var result = MapToResponse(clinic, branchDetails, mappedServices);
        return Result.Success(result);
    }

    private static Response.GetClinicDetail MapToResponse(Clinic? clinic,
        List<Response.GetClinicDetail>? branches = null,
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
}