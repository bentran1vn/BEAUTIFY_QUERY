using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Feedback;
using Feedback = BEAUTIFY_QUERY.DOMAIN.Documents.Feedback;
using User = BEAUTIFY_QUERY.DOMAIN.Documents.User;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.Feedbacks;
public class CreateFeedbackEventHandler(IMongoRepository<ClinicServiceProjection> clinicServiceRepository)
    : ICommandHandler<DomainEvents.CreateFeedback>
{
    public async Task<Result> Handle(DomainEvents.CreateFeedback request, CancellationToken cancellationToken)
    {
        var serviceRequest = request.entity;

        // Fetch the existing service (throw exception if not found)
        var isServiceExisted = await clinicServiceRepository
                                   .FindOneAsync(p => p.DocumentId == serviceRequest.ServiceId)
                               ?? throw new Exception($"Service {serviceRequest.ServiceId} not found");

        var feedbacks = isServiceExisted.Feedbacks;

        var feedback = new Feedback
        {
            FeedbackId = serviceRequest.FeedbackId,
            Content = serviceRequest.Content,
            CreatedAt = serviceRequest.CreatedAt,
            Images = serviceRequest.Images,
            Rating = serviceRequest.Rating,
            IsView = true,
            User = new User
            {
                Id = serviceRequest.User.Id,
                PhoneNumber = serviceRequest.User.PhoneNumber,
                Address = serviceRequest.User.Address,
                Avatar = serviceRequest.User.Avatar,
                FirstName = serviceRequest.User.FirstName,
                FullName = serviceRequest.User.FullName,
                LastName = serviceRequest.User.LastName
            }
        };

        feedbacks.Add(feedback);

        isServiceExisted.Feedbacks = feedbacks;

        await clinicServiceRepository.ReplaceOneAsync(isServiceExisted);

        return Result.Success();
    }
}