using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Booking;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using MongoDB.Driver;
using Clinic = BEAUTIFY_QUERY.DOMAIN.Entities.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetBookingDetailByIdQueryHandler(
    IMongoRepository<CustomerScheduleProjection> mongoRepository,
    IRepositoryBase<Staff, Guid> staffRepository,
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
            mongoRepository.AsQueryable(x => x.OrderId == booking.OrderId && x.Id != booking.Id)
                .ToListAsync(cancellationToken);

        var doctorImageUrlTask = staffRepository.FindSingleAsync(x => x.Id == booking.DoctorId, cancellationToken);
        var clinicImageUrlTask = clinicRepository.FindSingleAsync(x => x.Id == booking.ClinicId, cancellationToken);
        await Task.WhenAll(doctorImageUrlTask, clinicImageUrlTask);
        var procedureHistory = allBookingsBelongingToCustomer
            .Select(x => new Response.ProcedureHistory
            {
                Name = x.CurrentProcedure.Name,
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
                Name = booking.ServiceName,
            },
            Doctor = new Response.DoctorResponse
            {
                Id = booking.DoctorId,
                Name = booking.DoctorName,
                ImageUrl = doctorImageUrlTask?.Result?.ProfilePicture
            },
            Clinic = new Response.ClinicResponse
            {
                Id = booking.ClinicId,
                Name = booking.ClinicName,
                ImageUrl = clinicImageUrlTask.Result?.ProfilePictureUrl
            },
            ProcedureHistory = procedureHistory
        };
        return Result.Success(response);
    }
}