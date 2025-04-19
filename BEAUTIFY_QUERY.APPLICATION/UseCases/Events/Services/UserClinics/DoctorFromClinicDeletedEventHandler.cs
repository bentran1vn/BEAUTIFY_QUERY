using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.Clinic;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.UserClinics;
public class DoctorFromClinicDeletedEventHandler(
    IMongoRepository<ClinicServiceProjection> clinicServiceRepository
    /*,IMongoRepository<WorkingScheduleProjection> workingScheduleRepository,
      IMongoRepository<CustomerScheduleProjection> customerScheduleRepository*/)
    : ICommandHandler<DomainEvents.DoctorFromClinicDeleted>
{
    public async Task<Result> Handle(DomainEvents.DoctorFromClinicDeleted request, CancellationToken cancellationToken)
    {
        // 1. Remove doctor from all clinic services
        var clinicServices = clinicServiceRepository
            .FilterBy(x => x.DoctorServices.Any(ds => ds.Doctor.Id == request.IdDoctor));

        foreach (var service in clinicServices)
        {
            // Find all doctor services for this doctor and remove them
            var doctorServicesToRemove = service.DoctorServices
                .Where(ds => ds.Doctor.Id == request.IdDoctor)
                .ToList();

            foreach (var doctorService in doctorServicesToRemove) service.DoctorServices.Remove(doctorService);

            // Save changes if any doctor services were removed
            if (doctorServicesToRemove.Count != 0) await clinicServiceRepository.ReplaceOneAsync(service);
        }
/*
        // 2. Mark all working schedules for this doctor as deleted
        var workingSchedules = workingScheduleRepository.FilterBy(x => x.DoctorId == request.IdDoctor);
        foreach (var schedule in workingSchedules)
        {
            schedule.IsDeleted = true;
            await workingScheduleRepository.ReplaceOneAsync(schedule);
        }

        // 3. Update customer schedules for this doctor (mark as unavailable)
        var customerSchedules = customerScheduleRepository.FilterBy(x => x.DoctorId == request.IdEvent);
        foreach (var schedule in customerSchedules)
        {
            // Update status to indicate doctor is no longer available
            schedule.Status = "DoctorUnavailable";
            await customerScheduleRepository.ReplaceOneAsync(schedule);
        }*/

        return Result.Success();
    }
}