using AutoMapper;
using ClosedXML.Excel;
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
[Route("api/{version:apiVersion}/surveys")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SurveysController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Получение опросов с пагинацией
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<SelectModel<SurveyModel>>> GetSurveys(int page = 1, int size = 10,
        string? search = null)
    {
        if (page < 1 || size < 1)
        {
            ModelState.AddModelError("", "страница и размер должны быть больше 1");
            return BadRequest(ModelState);
        }

        // TODO add survey filtering if user is not admin
        var user = await dbContext.Users.Include(u => u.Groups).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id.ToString() == User.GetId());

        var surveys = dbContext
            .Surveys
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(search)) surveys = surveys.Where(s => EF.Functions.Like(s.Title, $"%{search}%"));

        return new SelectModel<SurveyModel>
        {
            Items = surveys.OrderByDescending(s => s.Title)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => mapper.Map<SurveyModel>(s)).ToList(),
            TotalCount = await surveys.CountAsync()
        };
    }

    /// <summary>
    ///     Получение деталки опроса
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SurveyModel), 200)]
    public async Task<IActionResult> GetSurvey(Guid id)
    {
        var survey = await dbContext.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Answers)
            .Include(s => s.Questions)
            .ThenInclude(q => q.Groups)
            .Include(s => s.Questions)
            .ThenInclude(q => q.Users)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (survey == null) return NotFound();

        return Ok(mapper.Map<SurveyModel>(survey));
    }

    /// <summary>
    ///     Создание опроса
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
    [HttpPost]
    public async Task<IActionResult> CreateSurvey([FromBody] SurveyWriteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var survey = mapper.Map<Survey>(model);

        dbContext.Surveys.Add(survey);
        await dbContext.SaveChangesAsync();

        return Ok();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
    [HttpPut("{id}")]
    public async Task<ActionResult<SurveyModel>> UpdateSurvey(Guid id, [FromBody] SurveyWriteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var survey = await dbContext.Surveys.FirstOrDefaultAsync(s => s.Id == id);

        if (survey == null) return NotFound();

        survey.Title = model.Title;
        survey.JsonContent = model.JsonContent;

        await dbContext.SaveChangesAsync();

        return mapper.Map<SurveyModel>(survey);
    }

    /// <summary>
    ///     Создание вопросов
    /// </summary>
    /// <param name="surveyId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("{surveyId}/questions")]
    public async Task<ActionResult<SurveyQuestionModel>> CreateSurveyQuestion(Guid surveyId,
        [FromBody] SurveyQuestionWriteModel model)
    {
        var survey = await dbContext.Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return NotFound();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var question = mapper.Map<SurveyQuestion>(model);

        foreach (var usrId in model.UserIds)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == usrId);

            if (user == null) return NotFound();

            question.Users.Add(user);
        }

        foreach (var grpId in model.GroupIds)
        {
            var group = await dbContext.Groups.FirstOrDefaultAsync(u => u.Id == grpId);

            if (group == null) return NotFound();

            question.Groups.Add(group);
        }

        survey.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        return mapper.Map<SurveyQuestionModel>(question);
    }

    [HttpPut("{surveyId}/questions/{questionId}")]
    public async Task<ActionResult<SurveyQuestionModel>> UpdateSurveyQuestion(
        Guid surveyId,
        Guid questionId,
        [FromBody] SurveyQuestionWriteModel model)
    {
        var survey = await dbContext.Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return NotFound();

        var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question == null) return NotFound();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        mapper.Map(model, question);

        question.Users.Clear();
        question.Groups.Clear();

        foreach (var usrId in model.UserIds)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == usrId);

            if (user == null) return NotFound();

            question.Users.Add(user);
        }

        foreach (var grpId in model.GroupIds)
        {
            var group = await dbContext.Groups.FirstOrDefaultAsync(u => u.Id == grpId);

            if (group == null) return NotFound();

            question.Groups.Add(group);
        }

        survey.Questions[survey.Questions.IndexOf(question)] = question;
        await dbContext.SaveChangesAsync();

        return mapper.Map<SurveyQuestionModel>(question);
    }

    [HttpDelete("{surveyId}/questions/{questionId}")]
    public async Task<ActionResult> DeleteSurveyQuestion(
        Guid surveyId,
        Guid questionId)
    {
        var survey = await dbContext.Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return NotFound();

        var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question == null) return NotFound();

        survey.Questions.Remove(question);
        await dbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Получение отчёта
    /// </summary>
    /// <param name="surveyId"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
    [HttpGet("report")]
    public async Task<IActionResult> GetReport(Guid surveyId)
    {
        var survey = await dbContext.Surveys.AsNoTracking().Include(s => s.Questions.OrderBy(q => q.Title))
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return NotFound();

        var sessions = await dbContext.SurveySessions
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Answers)
            .ThenInclude(a => a.Answer)
            .Where(s => s.SurveyId == surveyId && s.End != null)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Результаты");
        var currentRow = 1;
        var column = 1;

        worksheet.Cell(currentRow, column++).Value = "Email";

        foreach (var question in survey.Questions) worksheet.Cell(currentRow, column++).Value = question.Title;

        worksheet.Cell(currentRow, column++).Value = "Время прохождения";

        currentRow++;

        foreach (var answer in sessions)
        {
            column = 1;
            worksheet.Cell(currentRow, column++).Value = answer.User.Email;

            foreach (var question in survey.Questions)
            {
                switch (question.Type)
                {
                    case SurveyQuestionType.Single:
                    case SurveyQuestionType.Multiple:
                    {
                        worksheet.Cell(currentRow, column++).Value = string.Join(", ",
                            answer.Answers.Where(a => a.QuestionId == question.Id)
                                .Select(s => s.Answer.Text));
                        break;
                    }
                    case SurveyQuestionType.Text:
                    {
                        worksheet.Cell(currentRow, column++).Value =
                            answer.Answers.FirstOrDefault(a => a.QuestionId == question.Id)?.TextAnswer ?? "";
                        break;
                    }
                    default:
                    {
                        worksheet.Cell(currentRow, column++).Value = "Unsupported";
                        break;
                    }
                }
            }

            var timespan = (answer.End - answer.Start).Value;
            worksheet.Cell(currentRow, column++).Value =
                $"{(int)timespan.TotalDays}д, {timespan.Hours} ч {timespan.Minutes}м {timespan.Seconds}с";

            currentRow++;
        }

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{survey.Title} - Результаты.xlsx");
    }
}