using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;
internal sealed class GetServiceByCategoryIdQueryHandler(
    IMongoRepository<ClinicServiceProjection> repository,
    IRepositoryBase<DoctorService, Guid> doctorServiceRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository)
    : IQueryHandler<Query.GetServiceByCategoryIdQuery, List<Response.GetAllServiceByIdResponse>>
{
    public async Task<Result<List<Response.GetAllServiceByIdResponse>>> Handle(
        Query.GetServiceByCategoryIdQuery request,
        CancellationToken cancellationToken)
    {
        var clinicServices = repository
            .FilterBy(x => x.Category.Id == request.CategoryId)
            .OrderBy(x => x.Name)
            .ToList();

        if (clinicServices.Count == 0)
            return Result.Failure<List<Response.GetAllServiceByIdResponse>>(
                new Error("404", "No services found for the given category."));

        // Extract ServiceIds and load related DoctorServices
        var serviceIds = clinicServices.Select(x => x.DocumentId).ToList();
        var doctors = await doctorServiceRepository
            .FindAll(x => serviceIds.Contains(x.ServiceId))
            .ToListAsync(cancellationToken);

        // Load DoctorCertificates for the fetched doctors
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
            x.CoverImage?.Select(img => new Response.Image(img.Id, img.Index, img.Url)).ToList() ?? new List<Response.Image>(),
            x.Clinic?.Select(clinic => new Response.Clinic(
                clinic.Id,
                clinic.Name,
                clinic.Email,
                clinic.Address,
                clinic.PhoneNumber,
                clinic.ProfilePictureUrl,
                clinic.IsParent,
                clinic.IsActivated,
                clinic.ParentId)).ToList() ?? new List<Response.Clinic>(),
            new Response.Category(
                x.Category.Id,
                x.Category.Name,
                x.Category.Description),
            x.Procedures?.Select(proc => new Response.Procedure(
                proc.Id,
                proc.Name,
                proc.Description,
                proc.StepIndex,
                proc.ProcedurePriceTypes?.Select(pt => new Response.ProcedurePriceType(
                    pt.Id,
                    pt.Name,
                    pt.Duration,
                    pt.Price,
                    pt.IsDefault)).ToList() ?? new List<Response.ProcedurePriceType>())).ToList() ?? new List<Response.Procedure>(),
            x.Promotions?
                .Where(p => p.IsActivated)
                .Select(promo => new Response.Promotion(
                    promo.Id,
                    promo.Name,
                    promo.DiscountPercent,
                    promo.ImageUrl,
                    promo.StartDay,
                    promo.EndDate,
                    promo.IsActivated))
                .ToList() ?? new List<Response.Promotion>(),
            x.DoctorServices?
                .Select(ds => new Response.DoctorService(
                    ds.Id,
                    ds.ServiceId,
                    new Response.UserEntity(
                        ds.Doctor.Id,
                        ds.Doctor.FullName,
                        ds.Doctor.Email,
                        ds.Doctor.PhoneNumber,
                        ds.Doctor.ProfilePictureUrl,
                        doctorCertificates
                            .Where(c => c.DoctorId == ds.Doctor.Id)
                            .Select(c => new Response.CertificateEntity
                            {
                                Id = c.Id,
                                CertificateUrl = c.CertificateUrl,
                                CertificateName = c.CertificateName,
                                ExpiryDate = c.ExpiryDate,
                                Note = c.Note
                            }).ToList()))).ToList() ?? new List<Response.DoctorService>(),
            x.Feedbacks.Select(z => new Response.Feedback()
            {
                FeedbackId = z.FeedbackId,
                ServiceId = z.ServiceId,
                Content = z.Content,
                IsView = z.IsView,
                Rating = z.Rating,
                User = new Response.User()
                {
                    Id = z.User.Id,
                    FullName = z.User.FullName,
                    LastName = z.User.LastName,
                    FirstName = z.User.FirstName,
                    Address = z.User.Address,
                    PhoneNumber = z.User.PhoneNumber,
                    Avatar = z.User.Avatar
                },
                Images = z.Images,
                CreatedAt = z.CreatedAt,
                UpdatedAt = z.UpdatedAt
            }).ToList()
        )).ToList();

        return Result.Success(services);
    }
}