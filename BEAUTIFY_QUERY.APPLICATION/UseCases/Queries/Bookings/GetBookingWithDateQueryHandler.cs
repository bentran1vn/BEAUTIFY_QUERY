using BEAUTIFY_QUERY.CONTRACT.Services.Booking;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Bookings;
internal sealed class GetBookingWithDateQueryHandler(
    IMongoRepository<CustomerScheduleProjection> customerMongoRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query.GetBookingWithDate, Response.GetBookingWithDateResponse>
{
    public async Task<Result<Response.GetBookingWithDateResponse>> Handle(
        Query.GetBookingWithDate request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
            return Result.Failure<Response.GetBookingWithDateResponse>(new Error("400", "Invalid date format"));

        var customerSchedules = customerMongoRepository
            .FilterBy(x => x.Date!.Value.Equals(date) && x.ClinicId == currentUserService.ClinicId);

        return Result.Success(MapToResponse(customerSchedules.ToList()));
    }

    private static Response.GetBookingWithDateResponse MapToResponse(List<CustomerScheduleProjection> projections)
    {
        var appointments = projections.ConvertAll(x => new Response.AppointmentResponse
        {
            Id = x.DocumentId,
            Customer =
                new Response.CustomerResponse { Id = x.CustomerId, Name = x.CustomerName, Avatar = string.Empty },
            Service = new Response.ServiceResponse { Id = x.ServiceId, Name = x.ServiceName },
            Doctor = new Response.DoctorResponse { Id = x.DoctorId, Name = x.DoctorName! },
            Clinic = new Response.ClinicResponse { Id = x.ClinicId, Name = x.ClinicName },
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Duration = (x.EndTime - x.StartTime).ToString()!,
            Status = x.Status
        });
        return new Response.GetBookingWithDateResponse
        {
            Date = projections.FirstOrDefault()?.Date!.Value.ToString("yyyy-MM-dd")!,
            Appointments = appointments
        };
    }
}