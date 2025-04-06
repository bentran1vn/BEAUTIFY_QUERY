using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Services;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Services;
public class GetClinicServicesByIdQueryHandler(
    IMongoRepository<ClinicServiceProjection> clinicServiceRepository,
    IRepositoryBase<DoctorCertificate, Guid> doctorCertificateRepository
)
    : IQueryHandler<Query.GetClinicServicesByIdQuery, Response.GetAllServiceByIdResponse>
{
    public async Task<Result<Response.GetAllServiceByIdResponse>> Handle(
        Query.GetClinicServicesByIdQuery request,
        CancellationToken cancellationToken)
    {
        var isServiceExisted = await clinicServiceRepository.FindOneAsync(p => p.DocumentId.Equals(request.ServiceId));

        if (isServiceExisted == null)
            throw new Exception($"Service {request.ServiceId} not found");
        var listDoctorID = isServiceExisted.DoctorServices.Select(x => x.Doctor.Id).ToList();
        var doctorCertificates = await doctorCertificateRepository
            .FindAll(x => listDoctorID.Contains(x.DoctorId))
            .ToListAsync(cancellationToken);

        Response.GetAllServiceByIdResponse result;

        if (request.MainClinicId != null)
            result = new Response.GetAllServiceByIdResponse(
                isServiceExisted.DocumentId,
                isServiceExisted.Name,
                isServiceExisted.Description,
                new Response.Clinic(
                    isServiceExisted.Branding.Id,
                    isServiceExisted.Branding.Name,
                    isServiceExisted.Branding.Email,
                    isServiceExisted.Branding.Address,
                    isServiceExisted.Branding.PhoneNumber,
                    isServiceExisted.Branding.ProfilePictureUrl,
                    isServiceExisted.Branding.IsParent,
                    isServiceExisted.Branding.ParentId),
                isServiceExisted.MaxPrice,
                isServiceExisted.MinPrice,
                (isServiceExisted.DiscountPercent * 100).ToString(),
                isServiceExisted.DiscountMaxPrice,
                isServiceExisted.DiscountMinPrice,
                isServiceExisted.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
                isServiceExisted.Clinic.Select(y => new Response.Clinic(
                    y.Id,
                    y.Name,
                    y.Email,
                    y.Address,
                    y.PhoneNumber,
                    y.ProfilePictureUrl,
                    y.IsParent,
                    y.ParentId)).ToList(),
                new Response.Category(
                    isServiceExisted.Category.Id,
                    isServiceExisted.Category.Name,
                    isServiceExisted.Category.Description),
                isServiceExisted.Procedures.Select(x => new Response.Procedure(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.StepIndex,
                    x.ProcedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                        y.Id,
                        y.Name,
                        y.Duration,
                        y.Price,
                        y.IsDefault)).ToList())).OrderBy(x => x.StepIndex).ToList(),
                isServiceExisted.Promotions.Select(x => new Response.Promotion(
                    x.Id,
                    x.Name,
                    x.DiscountPercent,
                    x.ImageUrl,
                    x.StartDay,
                    x.EndDate,
                    x.IsActivated)).ToList(),
                isServiceExisted.DoctorServices.Select(y => new Response.DoctorService(
                    y.Id,
                    y.ServiceId,
                    new Response.UserEntity(
                        y.Doctor.Id,
                        y.Doctor.FullName,
                        y.Doctor.Email,
                        y.Doctor.PhoneNumber,
                        y.Doctor.ProfilePictureUrl,
                        doctorCertificates
                            .Where(c => c.DoctorId == y.Doctor.Id)
                            .Select(c => new Response.CertificateEntity
                            {
                                Id = c.Id,
                                CertificateUrl = c.CertificateUrl,
                                CertificateName = c.CertificateName,
                                ExpiryDate = c.ExpiryDate,
                                Note = c.Note
                            }).ToList()
                    ))).ToList());
        else
            result = new Response.GetAllServiceByIdResponse(
                isServiceExisted.DocumentId,
                isServiceExisted.Name,
                isServiceExisted.Description,
                new Response.Clinic(
                    isServiceExisted.Branding.Id,
                    isServiceExisted.Branding.Name,
                    isServiceExisted.Branding.Email,
                    isServiceExisted.Branding.Address,
                    isServiceExisted.Branding.PhoneNumber,
                    isServiceExisted.Branding.ProfilePictureUrl,
                    isServiceExisted.Branding.IsParent,
                    isServiceExisted.Branding.ParentId),
                isServiceExisted.MaxPrice,
                isServiceExisted.MinPrice,
                (isServiceExisted.DiscountPercent * 100).ToString(),
                isServiceExisted.DiscountMaxPrice,
                isServiceExisted.DiscountMinPrice,
                isServiceExisted.CoverImage.Select(x => new Response.Image(x.Id, x.Index, x.Url)).ToList(),
                isServiceExisted.Clinic.Select(y => new Response.Clinic(
                    y.Id,
                    y.Name,
                    y.Email,
                    y.Address,
                    y.PhoneNumber,
                    y.ProfilePictureUrl,
                    y.IsParent,
                    y.ParentId)).ToList(),
                new Response.Category(
                    isServiceExisted.Category.Id,
                    isServiceExisted.Category.Name,
                    isServiceExisted.Category.Description),
                isServiceExisted.Procedures.Select(x => new Response.Procedure(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.StepIndex,
                    x.ProcedurePriceTypes.Select(y => new Response.ProcedurePriceType(
                        y.Id,
                        y.Name,
                        y.Duration,
                        y.Price,
                        y.IsDefault)).ToList())).OrderBy(x => x.StepIndex).ToList(),
                null,
                isServiceExisted.DoctorServices.Select(y => new Response.DoctorService(
                    y.Id,
                    y.ServiceId,
                    new Response.UserEntity(
                        y.Doctor.Id,
                        y.Doctor.FullName,
                        y.Doctor.Email,
                        y.Doctor.PhoneNumber,
                        y.Doctor.ProfilePictureUrl,
                        doctorCertificates
                            .Where(c => c.DoctorId == y.Doctor.Id)
                            .Select(c => new Response.CertificateEntity
                            {
                                Id = c.Id,
                                CertificateUrl = c.CertificateUrl,
                                CertificateName = c.CertificateName,
                                ExpiryDate = c.ExpiryDate,
                                Note = c.Note
                            }).ToList()))).ToList());

        return Result.Success(result);
    }
}