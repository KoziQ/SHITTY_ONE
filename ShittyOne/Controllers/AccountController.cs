using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;
using ShittyOne.Models;
using ShittyOne.Services;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/account")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AccountController(AppDbContext dbContext, IMapper mapper, IEmailService emailService) : ControllerBase
{
    /// <summary>
    ///     Получение сообщение с опросом
    /// </summary>
    /// <param name="surveyId"></param>
    /// <returns></returns>
    [HttpGet("{surveyId}/Email")]
    public async Task<ActionResult> GetSurveyByEmail(Guid surveyId)
    {
        var survey = await dbContext.Surveys
            .Include(s =>
                s.Questions.Where(q => q.Users.Any(u => u.Id.ToString() == User.GetId())).OrderBy(q => q.Title))
            .ThenInclude(q => q.File)
            .Include(s => s.Questions)
            .ThenInclude(l => l.Answers.OrderBy(a => a.Text))
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null)
        {
            return NotFound();
        }

        var result = await emailService.SendEmail("SurveyEmail", User.Identity.Name!, survey.Title,
            mapper.Map<SurveyModel>(survey));

        return Ok(result);
    }
}