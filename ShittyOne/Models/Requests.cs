using System.ComponentModel.DataAnnotations;
using ShittyOne.Entities;

namespace ShittyOne.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Логин - обязательное поле")]
    [MaxLength(255)]
    public string Login { get; set; }

    [Required(ErrorMessage = "Пароль - обязательное поле")]
    public string Password { get; set; }
}

public class UserGroupWriteModel
{
    [Required(ErrorMessage = "Название группы - обязательное поле")]
    [MaxLength(255, ErrorMessage = "Длина символов до {1}")]
    public string Title { get; set; }

    public List<Guid> UserIds { get; set; }
}

public class SurveyWriteModel
{
    [Required(ErrorMessage = "Заголовок - обязательное поле")]
    public string Title { get; set; }

    public string JsonContent { get; set; }

    public Guid? FileId { get; set; }
}

public class SurveyQuestionWriteModel
{
    [Required(ErrorMessage = "Заголовок - обязательное поле")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Тип вопроса - обязательное поле")]
    public SurveyQuestionType Type { get; set; }

    public List<QuestionAnswerWriteModel> Answers { get; set; } = new();

    public List<Guid> UserIds { get; set; }
    public List<Guid> GroupIds { get; set; }
}

public class QuestionAnswerWriteModel
{
    public Guid? Id { get; set; }
    public string Text { get; set; }
}

public class SubmitQuestionAnswerWriteModel
{
    public List<Guid> Answers { get; set; }
    public string? Text { get; set; }
}

public class SurveySessionWriteModel
{
    public Guid SurveyId { get; set; }
}

public class UserWriteModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}