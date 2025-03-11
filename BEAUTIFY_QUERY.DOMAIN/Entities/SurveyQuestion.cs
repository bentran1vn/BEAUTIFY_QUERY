using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class SurveyQuestion : AggregateRoot<Guid>, IAuditableEntity
{
    public string? Question { get; set; }

    [MaxLength(30)] public string? QuestionType { get; set; }

    public Guid? SurveyId { get; set; }
    public virtual Survey? Survey { get; set; }

    public virtual ICollection<SurveyAnswer>? SurveyAnswers { get; set; } = [];

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}