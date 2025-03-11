namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class Survey : AggregateRoot<Guid>, IAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<SurveyQuestion>? SurveyQuestions { get; set; } = [];
    
    public virtual SurveyResponse? SurveyResponse { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}