namespace ShittyOne.Entities
{
    public class SurveyQuestionAnswer
    {
        public Guid Id { get; set; }
        public Guid SuveyQuestionId { get; set; }
        public SurveyQuestion Question { get; set; }
        public string Text { get; set; }
    }
}
