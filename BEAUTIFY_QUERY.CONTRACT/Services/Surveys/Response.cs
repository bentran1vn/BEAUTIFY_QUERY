namespace BEAUTIFY_QUERY.CONTRACT.Services.Surveys;
public static class Response
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
    }
}