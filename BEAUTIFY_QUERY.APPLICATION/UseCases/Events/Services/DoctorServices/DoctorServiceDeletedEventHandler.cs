using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.DoctorServices;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.DoctorServices;
public class DoctorServiceDeletedEventHandler(IMongoRepository<ClinicServiceProjection> repository)
    : ICommandHandler<DomainEvents.DoctorServiceDeleted>
{
    public Task<Result> Handle(DomainEvents.DoctorServiceDeleted request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}