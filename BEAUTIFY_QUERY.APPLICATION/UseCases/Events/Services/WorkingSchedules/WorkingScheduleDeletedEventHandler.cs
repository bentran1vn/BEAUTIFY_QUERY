using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class WorkingScheduleDeletedEventHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : ICommandHandler<DomainEvents.WorkingScheduleDeleted>
{
    public async Task<Result> Handle(DomainEvents.WorkingScheduleDeleted request, CancellationToken cancellationToken)
    {
        await repository.DeleteOneAsync(x => x.DocumentId == request.WorkingId);
        return Result.Success();
    }
}