using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;
using ShittyOne.Data.Migrations;
using ShittyOne.Entities;
using ShittyOne.Models;
using System.Runtime.InteropServices;

namespace ShittyOne.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/Surveys/{surveyId}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CompleteController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public CompleteController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
        [HttpGet("Report")]
        public async Task<IActionResult> GetReport(Guid surveyId)
        {
            var survey = await _dbContext.Surveys.AsNoTracking().Include(s => s.Questions.OrderBy(q => q.Title)).FirstOrDefaultAsync(s => s.Id == surveyId);

            if(survey == null)
            {
                return NotFound();
            }

            var sessions = await _dbContext.UserSessions
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.Answer)
                .Where(s => s.SurveyId == surveyId && s.End != null)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Результаты");
            int currentRow = 1;
            int column = 1;

            worksheet.Cell(currentRow, column++).Value = "Email";
            worksheet.Cell(currentRow, column++).Value = "Время прохождения";

            foreach (var question in survey.Questions)
            {
                worksheet.Cell(currentRow, column++).Value = question.Title;
            }

            currentRow++;

            foreach(var answer in sessions)
            {
                column = 1;
                worksheet.Cell(currentRow, column++).Value = answer.User.Email;

                foreach(var question in survey.Questions)
                {
                    switch (question.GetType().Name)
                    {
                        case nameof(MultipleQuestion):
                            worksheet.Cell(currentRow, column++).Value = string.Join(", ", answer.Answers.Where(a => a.QuestionId == question.Id)
                                .Select(s => s.Answer.Text)); break;
                        case nameof(StringQuestion):
                            worksheet.Cell(currentRow, column++).Value = answer.Answers.FirstOrDefault(a => a.QuestionId == question.Id)?.TextAnswer ?? ""; break;
                        default:
                            worksheet.Cell(currentRow, column++).Value = "Unsupported"; break;
                    }
                }

                var timespan = (answer.End - answer.Start).Value;
                worksheet.Cell(currentRow, column++).Value = $"{(int)timespan.TotalDays}д, {timespan.Hours} ч {timespan.Minutes}м {timespan.Seconds}с";

                currentRow++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{survey.Title} - Результаты.xlsx");
            }
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SurveyModel), 200)]
        public async Task<IActionResult> GetSurveyQuestions(Guid surveyId)
        {
            var survey = await _dbContext.Surveys.AsNoTracking()
                .Include(s => s.Questions.Where(q => q.Users.Any(u => u.Id.ToString() == User.GetId())))
                    .ThenInclude(q => q.File)
                .Include(s => s.Questions)
                    .ThenInclude(l => (l as MultipleQuestion).Answers)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if(survey == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SurveyModel>(survey));
        }

        [HttpPost("{questionId}")]
        public async Task<IActionResult> CompleteQuestion(Guid questionId,Guid surveyId, [FromBody] PostUserAnswer model, Guid? sessionId = null)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == User.GetId());

            var question = await _dbContext.Set<SurveyQuestion>()
                .FirstOrDefaultAsync(q => q.Id == questionId && q.SurveyId == surveyId && (q.Users.Any(u => u.Id == user.Id) || q.Groups.Any(g => g.Users.Any(u => u.Id == user.Id))));

            if(question == null)
            {
                return NotFound();
            }

            UserSession session;

            if (sessionId != null)
            {
                session = await _dbContext.UserSessions.Include(s => s.Answers).FirstOrDefaultAsync(s => s.Id == sessionId && s.SurveyId == surveyId);

                if (session == null || session.End != null)
                {
                    return NotFound();
                }
            }
            else
            {
                session = new UserSession
                {
                    User = user,
                    SurveyId = surveyId
                };

                _dbContext.UserSessions.Add(session); 
            }

            switch (question.GetType().Name)
            {
                case nameof(StringQuestion): 
                    {

                        if(model.Text == null)
                        {
                            return BadRequest(ModelState);
                        }

                        session.Answers.RemoveAll(a => a.QuestionId == questionId);
                        session.Answers.Add(new UserAnswer
                        {
                            Question = question,
                            Session = session,
                            TextAnswer = model.Text
                        });
                    } 
                    break;
                case nameof(MultipleQuestion):
                    {
                        if (!model.Answers.Any())
                        {
                            return BadRequest(ModelState);
                        }

                        foreach (var answerId in model.Answers)
                        {
                            var temp = await _dbContext.SurveysAnswer.FirstOrDefaultAsync(s => s.Id == (answerId));

                            if (temp == null)
                            {
                                return NotFound();
                            }
                            session.Answers.RemoveAll(a => a.QuestionId == questionId && a.SurveyQuestionAnswerId == temp.Id);
                            session.Answers.Add(new UserAnswer
                            {
                                Question = question,
                                Session = session,
                                Answer = temp
                            });
                        }

                    } break;
                default: return NotFound();
            }

            var questions = _dbContext.Set<SurveyQuestion>().Where(q => q.Users.Any(u => u.Id.ToString() == User.GetId())).Count();
            //Check if session was completed
            if (session.Answers.GroupBy(a => a.QuestionId).Count() == questions)
            {
                session.End = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                SessionId = session.Id,
                Percentage = double.Round(((double)session.Answers.GroupBy(a => a.QuestionId).Count()*100/ questions), 2),
                Started = session.Start
            });
        }
    }
}
