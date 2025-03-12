namespace BEAUTIFY_QUERY.CONTRACT.Services.SurveyQuestions;
public static class Response
{
    public class SurveyQuestionResponse
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string QuestionType { get; set; }
        public Guid? SurveyId { get; set; }
    }
}