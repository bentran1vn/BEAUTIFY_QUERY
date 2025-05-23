﻿using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.DoctorServices;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.DoctorServices;
public class DoctorServiceDeletedEventHandler(IMongoRepository<ClinicServiceProjection> repository)
    : ICommandHandler<DomainEvents.DoctorServiceDeleted>
{
    public async Task<Result> Handle(DomainEvents.DoctorServiceDeleted request, CancellationToken cancellationToken)
    {
        foreach (var item in request.DoctorServiceIds)
        {
            var service = await repository.FindOneAsync(x => x.DocumentId == request.ServiceId);
            var doctorService = service.DoctorServices.FirstOrDefault(x => x.Id == item);
            service.DoctorServices.Remove(doctorService);
            await repository.ReplaceOneAsync(service);
        }

        return Result.Success();
    }
}