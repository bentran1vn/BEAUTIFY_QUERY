using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class SurveyQuestionOption : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid SurveyQuestionId { get; set; }
    public virtual SurveyQuestion? SurveyQuestion { get; set; }
    
    [MaxLength(200)]
    public string? Option { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}