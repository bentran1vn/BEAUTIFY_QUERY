using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.WorkingSchedules;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Events.Services.WorkingSchedules;
internal sealed class WorkingScheduleDeletedEventHandler(IMongoRepository<WorkingScheduleProjection> repository)
    : ICommandHandler<DomainEvents.WorkingScheduleDeleted>
{
    public async Task<Result> Handle(DomainEvents.WorkingScheduleDeleted request, CancellationToken cancellationToken)
    {
        var slot = await repository.FindOneAsync(x => x.DocumentId == request.WorkingId);
        slot.IsDeleted = true;
        await repository.ReplaceOneAsync(slot);
        return Result.Success();
    }
}