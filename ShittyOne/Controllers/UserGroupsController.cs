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
[Route("api/{version:apiVersion}/userGroups")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
public class UserGroupsController(IMapper mapper, AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    ///     Получение деталки группы
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserGroupModel>> GetUserGroup(Guid id)
    {
        var group = await dbContext.Groups
            .Include(g => g.Users)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return mapper.Map<UserGroupModel>(group);
    }

    /// <summary>
    ///     Получение групп с пагинацией
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<SelectModel<UserGroupModel>>> SelectUserGroups(int page = 1, int size = 10,
        string? search = null)
    {
        if (page < 1 || size < 1)
        {
            ModelState.AddModelError("", "страница и размер должны быть больше 1");
            return BadRequest(ModelState);
        }

        var groups = dbContext.Groups
            .Include(g => g.Users)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            groups = groups.Where(g => EF.Functions.Like(g.Title, $"%{search}%"));
        }

        return new SelectModel<UserGroupModel>
        {
            Items = await groups.OrderBy(g => g.Title)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(g => mapper.Map<UserGroupModel>(g))
                .ToListAsync(),
            TotalCount = await groups.CountAsync()
        };
    }

    /// <summary>
    ///     Получение пользователей
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(SelectModel<UserModel>), 200)]
    public async Task<ActionResult<SelectModel<UserModel>>> SelectGroupUsers(int page = 1, int size = 10,
        string? search = null)
    {
        if (page < 1 || size < 1)
        {
            ModelState.AddModelError("", "страница и размер должны быть больше 1");
            return BadRequest(ModelState);
        }

        var users = dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            users = users.Where(g =>
                EF.Functions.Like(g.UserName, $"%{search}%") ||
                EF.Functions.Like(g.Email, $"%{search}%"));
        }

        return new SelectModel<UserModel>
        {
            Items = await users.OrderBy(g => g.UserName)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(g => mapper.Map<UserModel>(g))
                .ToListAsync(),
            TotalCount = await users.CountAsync()
        };
    }

    /// <summary>
    ///     Создание группы
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateUserGroup([FromBody] UserGroupWriteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var group = mapper.Map<Group>(model);

        foreach (var userId in model.UserIds)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            group.Users.Add(user);
        }

        dbContext.Add((object)group);
        await dbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Изменение группы
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroupUsers(Guid id, [FromBody] UserGroupWriteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var group = await dbContext.Groups
            .Include(u => u.Users)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        mapper.Map(model, group);

        var usersToDelete = group.Users
            .Where(u => model.UserIds.All(i => i != u.Id))
            .ToList();

        var guidsToAdd = model.UserIds
            .Where(u => group.Users.All(i => i.Id != u))
            .ToList();

        usersToDelete.ForEach(u => group.Users.Remove(u));

        foreach (var usr in guidsToAdd)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == usr);

            if (user == null) return NotFound();

            group.Users.Add(user);
        }

        await dbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Удаление группы
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserGroup(Guid id)
    {
        var groupd = await dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

        if (groupd == null) return NotFound();

        dbContext.Groups.Remove(groupd);
        await dbContext.SaveChangesAsync();

        return Ok();
    }
}