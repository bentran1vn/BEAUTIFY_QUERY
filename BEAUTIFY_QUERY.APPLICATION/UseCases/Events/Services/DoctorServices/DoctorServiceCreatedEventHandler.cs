using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.DoctorServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.DoctorServices;
public class DoctorServiceCreatedEventHandler(IMongoRepository<ClinicServiceProjection> repository)
    : ICommandHandler<DomainEvents.DoctorServiceCreated>
{
    public async Task<Result> Handle(DomainEvents.DoctorServiceCreated request, CancellationToken cancellationToken)
    {
        foreach (var item in request.entity)
        {
            var service = await repository.FindOneAsync(x => x.DocumentId == item.ServiceId);
            service.DoctorServices.Add(item);
            await repository.ReplaceOneAsync(service);
        }

        return Result.Success();
    }
}