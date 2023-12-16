namespace ShittyOne.Models
{

    public class FileModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class GroupModel
    {
        public string Title { get; set; }
        public List<UserModel> Users { get; set; } = new List<UserModel>();
    }

    public class UserModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }

    public class SelectModel<T> where T : class
    {
        public List<T> Data { get; set; } = new List<T>();
        public bool More { get; set; }
    }

    public class SurveyGroupModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<SurveyModel> Surveys { get; set; }
    }

    public class SurveyModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string JsonContent { get; set; }
        public FileModel File { get; set; }
        public List<QuestionModel> Questions { get; set; }
    }

    public abstract class QuestionModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string JsonContent { get; set; }
        public SurveyModel Survey { get; set; }
    }

    public class StringQuestionModel : QuestionModel
    {

    }

    public class MultipleQuestionModel : QuestionModel
    {
        public List<SurveyQuestionAnswerModel> Answers { get; set; }
    }

    public class SurveyQuestionAnswerModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public QuestionModel Question { get; set; }
    }
}
