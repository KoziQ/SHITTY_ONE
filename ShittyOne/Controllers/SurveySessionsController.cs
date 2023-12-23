using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Models;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/surveySessions/{surveySessionId}")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SurveySessionsController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Получение опроса с вопросами
    /// </summary>
    /// <param name="surveyId"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(SurveyModel), 200)]
    public async Task<IActionResult> GetSurveyQuestions(Guid surveyId)
    {
        var survey = await dbContext.Surveys.AsNoTracking()
            .Include(s => s.Questions.Where(q => q.Users.Any(u => u.Id.ToString() == User.GetId())))
            .ThenInclude(q => q.File)
            .Include(s => s.Questions)
            .ThenInclude(l => l.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return NotFound();

        return Ok(mapper.Map<SurveyModel>(survey));
    }

    [HttpPost("")]
    public async Task<ActionResult<SurveySession>> CreateSurveySession(SurveySessionWriteModel model)
    {
        var survey = await dbContext.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Users)
            .FirstOrDefaultAsync(s => s.Id == model.SurveyId);

        if (survey == null) return NotFound();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == User.GetId());

        var session = new SurveySession
        {
            Survey = survey,
            User = user!
        };

        dbContext.SurveySessions.Add(session);

        await dbContext.SaveChangesAsync();

        return session;
    }

    /// <summary>
    ///     Ответ на вопрос
    /// </summary>
    /// <param name="questionId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("answers/{questionId}/submit")]
    public async Task<IActionResult> SubmitQuestionAnswer(Guid surveySessionId, Guid questionId,
        [FromBody] SubmitQuestionAnswerWriteModel model)
    {
        var surveySession = await dbContext.SurveySessions
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == surveySessionId);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == User.GetId());

        var question = await dbContext.Set<SurveyQuestion>()
            .FirstOrDefaultAsync(q =>
                q.Id == questionId && (q.Users.Any(u => u.Id == user.Id) ||
                                       q.Groups.Any(g => g.Users.Any(u => u.Id == user.Id))));

        if (question == null)
        {
            return NotFound();
        }

        switch (question.Type)
        {
            case SurveyQuestionType.Single:
            case SurveyQuestionType.Multiple:
            {
                if (!model.Answers.Any()) return BadRequest(ModelState);

                foreach (var answerId in model.Answers)
                {
                    var temp = await dbContext.SurveysAnswer.FirstOrDefaultAsync(s => s.Id == answerId);

                    if (temp == null) return NotFound();
                    surveySession.Answers.RemoveAll(a =>
                        a.QuestionId == questionId && a.SurveyQuestionAnswerId == temp.Id);
                    surveySession.Answers.Add(new UserAnswer
                    {
                        Question = question,
                        Session = surveySession,
                        Answer = temp
                    });
                }

                break;
            }
            case SurveyQuestionType.Text:
            {
                if (model.Text == null) return BadRequest(ModelState);

                surveySession.Answers.RemoveAll(a => a.QuestionId == questionId);
                surveySession.Answers.Add(new UserAnswer
                {
                    Question = question,
                    Session = surveySession,
                    TextAnswer = model.Text
                });

                break;
            }
            default:
            {
                return NotFound();
            }
        }

        var questions = dbContext.Set<SurveyQuestion>()
            .Count(q => q.Users.Any(u => u.Id.ToString() == User.GetId()));

        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            SessionId = surveySession.Id,
            Percentage =
                double.Round((double)surveySession.Answers.GroupBy(a => a.QuestionId).Count() * 100 / questions, 2),
            Started = surveySession.Start
        });
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteSurveySession(Guid surveySessionId)
    {
        throw new NotImplementedException();
    }
}