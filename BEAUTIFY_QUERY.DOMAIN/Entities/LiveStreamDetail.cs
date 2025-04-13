namespace BEAUTIFY_QUERY.DOMAIN.Entities;

public class LiveStreamDetail : AggregateRoot<Guid>, IAuditableEntity
{
    public int JoinCount { get; set; }
    public int MessageCount { get; set; }
    public int ReactionCount { get; set; }
    public int TotalActivities { get; set; }
    public int TotalBooking { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}
/*
 * JoinCount = joinCount,
   MessageCount = messageCount,
   ReactionCount = reactionCount,
   TotalActivities = joinCount + messageCount + reactionCount,
   TotalBooking = completedBookingCount
 */