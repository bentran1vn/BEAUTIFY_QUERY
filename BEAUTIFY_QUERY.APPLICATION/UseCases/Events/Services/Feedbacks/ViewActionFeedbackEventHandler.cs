using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Feedback;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Feedbacks;
public class ViewActionFeedbackEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    : ICommandHandler<DomainEvents.ViewActionFeedback>
{
    public async Task<Result> Handle(DomainEvents.ViewActionFeedback request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        // Fetch the existing service (throw exception if not found)
        var isServiceExisted = await clinicServiceRepository
                                   .FindOneAsync(p => p.DocumentId == serviceRequest.ServiceId)
                               ?? throw new Exception($"Service {serviceRequest.ServiceId} not found");

        isServiceExisted.Feedbacks = isServiceExisted.Feedbacks
            .Select(x =>
            {
                if (x.FeedbackId == serviceRequest.FeedbackId) x.IsView = serviceRequest.IsView;
                return x;
            }).ToList();

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}