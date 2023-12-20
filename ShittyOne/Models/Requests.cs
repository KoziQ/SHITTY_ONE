using System.ComponentModel.DataAnnotations;

namespace ShittyOne.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Логин - обязательное поле")]
    [MaxLength(255)]
    public string Login { get; set; }

    [Required(ErrorMessage = "Пароль - обязательное поле")]
    public string Password { get; set; }
}

public class TokenModel
{
    public string Access { get; set; }
    public string Refresh { get; set; }
}

public class PostGroupModel
{
    [Required(ErrorMessage = "Название группы - обязательное поле")]
    [MaxLength(255, ErrorMessage = "Длина символов до {1}")]
    public string Title { get; set; }

    public List<Guid> Users { get; set; }
}

public class PostSurveyModel
{
    [Required(ErrorMessage = "Заголовок - обязательное поле")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Контент - обязательное поле")]
    public string JsonContent { get; set; }

    public Guid? FileId { get; set; }
}

public class PostQuestionModel
{
    [Required(ErrorMessage = "Заголовок - обязательное поле")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Контент - обязательное поле")]
    public string JsonContent { get; set; }

    [Required(ErrorMessage = "Тип вопроса - обязательное поле")]
    public string Type { get; set; }

    public List<PostQuestionAnswerModel> Answers { get; set; } = new();
    public List<Guid> Users { get; set; }
    public List<Guid> Groups { get; set; }
}

public class PostQuestionAnswerModel
{
    public string Text { get; set; }
}

public class PostUserAnswer
{
    public List<Guid> Answers { get; set; }
    public string? Text { get; set; }
}