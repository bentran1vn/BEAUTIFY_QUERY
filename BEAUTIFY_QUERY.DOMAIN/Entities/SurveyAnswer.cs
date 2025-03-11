namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class SurveyAnswer : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? SurveyQuestionId { get; set; }
    public virtual SurveyQuestion? SurveyQuestion { get; set; }
    public Guid? SurveyResponseId { get; set; }
    public virtual SurveyResponse? SurveyResponse { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}