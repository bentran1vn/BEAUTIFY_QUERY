using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Promotion = BEAUTIFY_QUERY.DOMAIN.Entities.Promotion;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;
internal sealed class GetServiceByClinicIdQueryHandler(
    IMongoRepository<ClinicServiceProjection> repository,
    IRepositoryBase<DoctorService, Guid> doctorServiceRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository
   )
    : IQueryHandler<Query.GetServiceByClinicIdQuery, List<Response.GetAllServiceByIdResponse>>
{
    public async Task<Result<List<Response.GetAllServiceByIdResponse>>> Handle(Query.GetServiceByClinicIdQuery request,
        CancellationToken cancellationToken)
    {
        var clinicServices =
            repository.FilterBy(
                    x => x.Clinic.Any(x => x.Id == request.ClinicId) && x.Promotions.Any(x => x.IsActivated))
                .OrderBy(x => x.Name)
                .ToList();
        if (clinicServices.Count == 0)
            return Result.Failure<List<Response.GetAllServiceByIdResponse>>(
                new Error("404", "Clinic Service Not Found !"));

        // Extract the ServiceIds first to avoid complex LINQ in EF translation
        var serviceIds = clinicServices.Select(x => x.DocumentId).ToList();

        var doctors = await doctorServiceRepository.FindAll(x => serviceIds.Contains(x.ServiceId))
            .ToListAsync(cancellationToken);

        var doctorIds = doctors.Select(x => x.DoctorId).ToList();
        var doctorCertificates = await doctorCertificateRepository
            .FindAll(x => doctorIds.Contains(x.DoctorId))
            .ToListAsync(cancellationToken);


        var services = clinicServices.Select(x => new Response.GetAllServiceByIdResponse
            (
                x.DocumentId,
                x.Name,
                x.Description,
                new Response.Clinic(
                    x.Branding.Id,
                    x.Branding.Name,
                    x.Branding.Email,
                    x.Branding.Address,
                    x.Branding.PhoneNumber,
                    x.Branding.ProfilePictureUrl,
                    x.Branding.IsParent,
                    x.Branding.IsActivated,
                    x.Branding.ParentId),
                x.MaxPrice,
                x.MinPrice,
                x.DiscountPercent.ToString(),
                x.DiscountMaxPrice,
                x.DiscountMinPrice,
                x.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
                x.Clinic.Select(y => new Response.Clinic(
                    y.Id,
                    y.Name,
                    y.Email,
                    y.Address,
                    y.PhoneNumber,
                    y.ProfilePictureUrl,
                    y.IsParent,
                    y.IsActivated,
                    y.ParentId)).ToList(),
                new Response.Category
                (
                    x.Category.Id,
                    x.Category.Name,
                    x.Category.Description
                ),
                x.Procedures.Select(x => new Response.Procedure(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.StepIndex,
                    x.ProcedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                        y.Id,
                        y.Name,
                        y.Duration,
                        y.Price,
                        y.IsDefault)).ToList())).ToList(),
                x.Promotions
                    .Select(promo => new Response.Promotion(
                        promo.Id,
                        promo.Name,
                        promo.DiscountPercent,
                        promo.ImageUrl,
                        promo.StartDay,
                        promo.EndDate,
                        promo.IsActivated))
                    .ToList(),
                x.DoctorServices.Select(y => new Response.DoctorService(
                    y.Id,
                    y.ServiceId,
                    new Response.UserEntity(
                        y.Doctor.Id,
                        y.Doctor.FullName,
                        y.Doctor.Email,
                        y.Doctor.PhoneNumber,
                        y.Doctor.ProfilePictureUrl,
                        doctorCertificates.Where(z => z.DoctorId == y.Doctor.Id).Select(c =>
                            new Response.CertificateEntity
                            {
                                Id = c.Id,
                                CertificateUrl = c.CertificateUrl,
                                CertificateName = c.CertificateName,
                                ExpiryDate = c.ExpiryDate,
                                Note = c.Note
                            }).ToList()))).ToList())
        ).ToList();

        return Result.Success(services);
    }
}