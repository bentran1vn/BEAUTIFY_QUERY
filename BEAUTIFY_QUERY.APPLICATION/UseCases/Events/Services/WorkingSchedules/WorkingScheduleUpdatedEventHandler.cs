using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class WorkingScheduleUpdatedEventHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : ICommandHandler<DomainEvents.WorkingScheduleUpdated>
{
    public async Task<Result> Handle(DomainEvents.WorkingScheduleUpdated request, CancellationToken cancellationToken)
    {
        foreach (var x in request.WorkingScheduleEntities)
        {
            var working = await repository.FindOneAsync(y => y.DocumentId == x.Id);
            working.StartTime = x.StartTime;
            working.EndTime = x.EndTime;
            working.Date = x.Date;
            working.DoctorName = request.DoctorName;
            await repository.ReplaceOneAsync(working);
        }

        return Result.Success();
    }
}