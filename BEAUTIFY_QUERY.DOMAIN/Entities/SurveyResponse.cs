namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class SurveyResponse : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid SurveyId { get; set; }
    public virtual Survey? Survey { get; set; }
    public Guid? CustomerId { get; set; }
    public virtual User? Customer { get; set; }


    public virtual ICollection<SurveyAnswer> SurveyAnswers { get; set; } = [];


    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}