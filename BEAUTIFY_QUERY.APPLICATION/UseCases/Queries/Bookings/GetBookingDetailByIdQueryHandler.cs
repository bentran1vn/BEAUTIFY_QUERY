using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Booking;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using MongoDB.Driver;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetBookingDetailByIdQueryHandler(IMongoRepository<CustomerScheduleProjection> mongoRepository)
    : IQueryHandler<Query.GetBookingDetailById, List<Response.GetBookingDetailByIdResponse>>
{
    public async Task<Result<List<Response.GetBookingDetailByIdResponse>>> Handle(Query.GetBookingDetailById request,
        CancellationToken cancellationToken)
    {
        var booking = await mongoRepository.FindOneAsync(x => x.DocumentId == request.Id);
        if (booking is null)
            return Result.Failure<List<Response.GetBookingDetailByIdResponse>>(new Error("404", "Booking Not Found !"));
        var allBookingsBelongingToCustomer = await
            mongoRepository.AsQueryable(x => x.OrderId == booking.OrderId && x.Id != booking.Id)
                .ToListAsync(cancellationToken);
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
        var response = allBookingsBelongingToCustomer.Select(x => new Response.GetBookingDetailByIdResponse
        {
            Id = x.DocumentId,
            CustomerName = x.CustomerName,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Date = x.Date,
            Status = x.Status,
            Service = new Response.ServiceResponse
            {
                Id = x.ServiceId,
                Name = x.ServiceName
            },
            Doctor = new Response.DoctorResponse
            {
                Id = x.DoctorId,
                Name = x.DoctorName
            },
            Clinic = new Response.ClinicResponse
            {
                Id = x.ClinicId,
                Name = x.ClinicName
            },
            ProcedureHistory = procedureHistory
        }).ToList();
        return Result.Success(response);
    }
}