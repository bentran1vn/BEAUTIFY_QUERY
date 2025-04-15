using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Feedback;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

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
        
        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);
        
        return Result.Success();
    }
}