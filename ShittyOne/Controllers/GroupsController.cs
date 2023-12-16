using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Models;

namespace ShittyOne.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
    public class GroupsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public GroupsController(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получение деталки группы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GroupModel), 200)]
        public async Task<IActionResult> Detail(Guid id)
        {
            var group = await _dbContext.Groups.Include(g => g.Users).AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

            if(group == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<GroupModel>(group));
        }

        /// <summary>
        /// Получение групп с пагинацией
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SelectModel<GroupModel>), 200)]
        public IActionResult Select(int page = 1, int size = 10, string? search = null)
        {
            if(page < 1 || size < 1)
            {
                ModelState.AddModelError("", "страница и размер должны быть больше 1");
                return BadRequest(ModelState);    
            }

            var groups = _dbContext.Groups.Include(g => g.Users).AsNoTracking().AsQueryable();

            if(!string.IsNullOrEmpty(search))
            {
                groups = groups.Where(g => EF.Functions.Like(g.Title, $"%{search}%"));
            }

            var total = groups.Count();

            return Ok(new SelectModel<GroupModel>
            {
                Data = groups.OrderBy(g => g.Title).Skip((page-1)*size).Take(size).Select(g => _mapper.Map<GroupModel>(g)).ToList(),
                More = total > page*size
            });
        }

        /// <summary>
        /// Получение пользователей
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet("Users")]
        [ProducesResponseType(typeof(SelectModel<UserModel>), 200)]
        public IActionResult SelectUsers(int page = 1, int size = 10, string? search = null)
        {
            if (page < 1 || size < 1)
            {
                ModelState.AddModelError("", "страница и размер должны быть больше 1");
                return BadRequest(ModelState);
            }

            var users = _dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(g => EF.Functions.Like(g.UserName, $"%{search}%") || EF.Functions.Like(g.Email, $"%{search}%"));
            }

            var total = users.Count();

            return Ok(new SelectModel<UserModel>
            {
                Data = users.OrderBy(g => g.UserName).Skip((page - 1) * size).Take(size).Select(g => _mapper.Map<UserModel>(g)).ToList(),
                More = total > page * size
            });
        }

        /// <summary>
        /// Создание группы
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PostGroupModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var group = _mapper.Map<Group>(model);

            foreach (var userId in model.Users)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if(user == null)
                {
                    return NotFound();
                }

                group.Users.Add(user);
            }

            _dbContext.Add(group);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Изменение группы
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] PostGroupModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var group = await _dbContext.Groups.Include(u => u.Users).FirstOrDefaultAsync(g => g.Id == id);

            if(group == null)
            {
                return NotFound();
            }

            _mapper.Map(model, group);

            var usersToDelete = group.Users.Where(u => !model.Users.Any(i => i == u.Id)).ToList();
            var guidsToAdd = model.Users.Where(u => !group.Users.Any(i => i.Id == u)).ToList();

            usersToDelete.ForEach(u => group.Users.Remove(u));

            foreach(var usr in guidsToAdd)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == usr);

                if(user == null)
                {
                    return NotFound();
                }

                group.Users.Add(user);
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Удаление группы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var groupd = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

            if (groupd == null)
            {
                return NotFound();
            }

            _dbContext.Groups.Remove(groupd);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
