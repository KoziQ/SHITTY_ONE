namespace ShittyOne.Models;

public class TokenModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class FileModel
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public DateTime CreationDate { get; set; }
}

public class UserGroupModel
{
    public string Title { get; set; }
    public List<UserModel> Users { get; set; } = new();
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
    public List<T> Items { get; set; } = new();
    public long TotalCount { get; set; }
}

public class SurveyModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string JsonContent { get; set; }
    public FileModel File { get; set; }
    public List<SurveyQuestionModel> Questions { get; set; }
}

public abstract class SurveyQuestionModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string JsonContent { get; set; }
    public SurveyModel Survey { get; set; }
    public List<UserModel> Users { get; set; } = new();
    public List<UserGroupModel> Groups { get; set; } = new();
}

public class SingleQuestionModel : SurveyQuestionModel
{
    public SurveyQuestionAnswerModel Answer { get; set; }
}

public class MultipleQuestionModel : SurveyQuestionModel
{
    public List<SurveyQuestionAnswerModel> Answers { get; set; }
}

public class SurveyQuestionAnswerModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }
}