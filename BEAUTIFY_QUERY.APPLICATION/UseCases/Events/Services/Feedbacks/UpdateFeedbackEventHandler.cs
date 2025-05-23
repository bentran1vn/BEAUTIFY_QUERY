using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Feedback;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Feedbacks;
public class UpdateFeedbackEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    : ICommandHandler<DomainEvents.UpdateFeedback>
{
    public async Task<Result> Handle(DomainEvents.UpdateFeedback request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        // Fetch the existing service (throw exception if not found)
        var isServiceExisted = await clinicServiceRepository
                                   .FindOneAsync(p => p.DocumentId == serviceRequest.ServiceId)
                               ?? throw new Exception($"Service {serviceRequest.ServiceId} not found");
        
        var doctor = isServiceExisted.DoctorServices;
        
        var doctorServiceRating = doctor.Select(x =>
        {
            var doctorService = serviceRequest.DoctorFeedbacks.FirstOrDefault(y => y.DoctorId == serviceRequest.User.Id);
            x.Rating = doctorService?.NewRating ?? 0;
            return x;
        });

        isServiceExisted.Feedbacks = isServiceExisted.Feedbacks
            .Select(x =>
            {
                if (x.FeedbackId == serviceRequest.FeedbackId)
                {
                    x.Content = serviceRequest.Content;
                    x.Images = serviceRequest.Images;
                    x.Rating = serviceRequest.Rating;
                    x.IsView = true;
                    x.UpdatedAt = serviceRequest.UpdateAt;
                }
                return x;
            }).ToList();
        
        isServiceExisted.Rating = serviceRequest.NewRating;
        isServiceExisted.DoctorServices = doctorServiceRating.ToList();

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}