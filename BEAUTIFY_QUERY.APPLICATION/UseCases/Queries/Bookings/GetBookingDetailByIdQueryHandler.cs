using BEAUTIFY_QUERY.CONTRACT.Services.Booking;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetBookingDetailByIdQueryHandler(
    IMongoRepository<CustomerScheduleProjection> mongoRepository,
    IRepositoryBase<UserClinic, Guid> userClinicRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository)
    : IQueryHandler<Query.GetBookingDetailById, Response.GetBookingDetailByIdResponse>
{
    public async Task<Result<Response.GetBookingDetailByIdResponse>> Handle(Query.GetBookingDetailById request,
        CancellationToken cancellationToken)
    {
        var booking = await mongoRepository.FindOneAsync(x => x.DocumentId == request.Id);
        if (booking is null)
            return Result.Failure<Response.GetBookingDetailByIdResponse>(new Error("404", "Booking Not Found !"));
        var allBookingsBelongingToCustomer = await
            IAsyncCursorSourceExtensions.ToListAsync(
                mongoRepository.AsQueryable(x => x.OrderId == booking.OrderId && x.Id != booking.Id),
                cancellationToken);
        var doctorImageUrl = await userClinicRepository.FindAll(x => x.UserId == booking.DoctorId).Include(x => x.User)
            .FirstOrDefaultAsync(cancellationToken);
        var clinicImageUrl = await clinicRepository.FindSingleAsync(x => x.Id == booking.ClinicId, cancellationToken);
        var procedureHistory = allBookingsBelongingToCustomer
            .Select(x => new Response.ProcedureHistory
            {
                ProcedureName = x.CurrentProcedure.ProcedureName,
                ProcedurePriceType = x.CurrentProcedure.Name,
                StepIndex = x.CurrentProcedure.StepIndex,
                DateCompleted = x.Date,
                TimeCompleted = x.EndTime,
                Duration = 0,
                Status = x.Status
            }).ToList();
        var response = new Response.GetBookingDetailByIdResponse
        {
            Id = booking.DocumentId,
            CustomerName = booking.CustomerName,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Date = booking.Date,
            Status = booking.Status,
            Service = new Response.ServiceResponse
            {
                Id = booking.ServiceId,
                Name = booking.ServiceName
            },
            Doctor = new Response.DoctorResponse
            {
                Id = booking.DoctorId,
                Name = booking.DoctorName,
                ImageUrl = doctorImageUrl?.User.ProfilePicture ?? string.Empty,
                Certificates = doctorImageUrl?.User.DoctorCertificates.Where(x => x.ServiceId == booking.ServiceId)
                    .Select(x => new Response.CertificateResponse
                    {
                        Id = x.Id,
                        Name = x.CertificateName,
                        ImageUrl = x.CertificateUrl
                    }).ToList()
            },
            Clinic = new Response.ClinicResponse
            {
                Id = booking.ClinicId,
                Name = booking.ClinicName,
                ImageUrl = clinicImageUrl?.ProfilePictureUrl
            },
            ProcedureHistory = procedureHistory
                .OrderBy(x => x.DateCompleted)
                .ThenBy(x => x.TimeCompleted)
                .ToList()
        };
        return Result.Success(response);
    }
}