using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Models;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShittyOne.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SurveysController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public SurveysController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SelectModel<SurveyModel>), 200)]
        public async Task<IActionResult> GetSurveys(int page = 1, int size = 10, string? search = null)
        {
            if (page < 1 || size < 1)
            {
                ModelState.AddModelError("", "страница и размер должны быть больше 1");
                return BadRequest(ModelState);
            }

            var user = await _dbContext.Users.Include(u => u.Groups).AsNoTracking().FirstOrDefaultAsync(u => u.Id.ToString() == User.GetId());

            var surveys = _dbContext
                .Surveys
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                surveys = surveys.Where(s => EF.Functions.Like(s.Title, $"%{search}%"));
            }

            var count = surveys.Count();

            return Ok(new SelectModel<SurveyModel>
            {
                Data = surveys.OrderByDescending(s => s.Title).Skip((page-1)*size).Take(size).Select(s => _mapper.Map<SurveyModel>(s)).ToList(),
                More = count > page*size
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SurveyModel), 200)]
        public async Task<IActionResult> Detail(Guid id)
        {
            var survey = await _dbContext.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => (q as MultipleQuestion).Answers)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Groups)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Users)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);

            if(survey == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SurveyModel>(survey));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PostSurveyModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var survey = _mapper.Map<Survey>(model);

            _dbContext.Surveys.Add(survey);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{surveyId}/Questions")]
        public async Task<IActionResult> AddQuestion(Guid surveyId, [FromBody] PostQuestionModel model)
        {
            var survey = await _dbContext.Surveys.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == surveyId);

            if(survey == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SurveyQuestion question;

            switch (model.Type)
            {
                case nameof(MultipleQuestion):
                    {
                        if (!model.Answers.Any())
                        {
                            ModelState.AddModelError(nameof(model.Answers), "Ответы - обязательное поле");
                            return BadRequest(ModelState);
                        }
                        question = _mapper.Map<MultipleQuestion>(model);
                        (question as MultipleQuestion).Answers.ForEach(a => a.Question = question);
                    } break;
                case nameof(StringQuestion):
                    {
                        question = _mapper.Map<StringQuestion>(model);
                    }
                    break;
                default:
                    {
                        ModelState.AddModelError(nameof(model.Type), "Невалидный тип вопроса");
                        return BadRequest(ModelState);
                    }
            }

            foreach (var usrId in model.Users)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == usrId);

                if (user == null)
                {
                    return NotFound();
                }

                question.Users.Add(user);
            }

            foreach (var grpId in model.Groups)
            {
                var group = await _dbContext.Groups.FirstOrDefaultAsync(u => u.Id == grpId);

                if (group == null)
                {
                    return NotFound();
                }

                question.Groups.Add(group);
            }

            survey.Questions.Add(question);
            await _dbContext.SaveChangesAsync();

            return Ok();
        } 
    }
}
