namespace BEAUTIFY_QUERY.CONTRACT.Services.Surveys;
public static class Response
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public List<SurveyQuestionResponse> Questions { get; set; }
    }

    public class SurveyQuestionResponse
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string QuestionType { get; set; }
        public List<SurveyQuestionOptionResponse> Options { get; set; }
    }

    public class SurveyQuestionOptionResponse
    {
        public string Option { get; set; }
    }
}