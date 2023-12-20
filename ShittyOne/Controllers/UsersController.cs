using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;
using ShittyOne.Models;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin))]
public class UsersController(AppDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SelectModel<UserModel>>> SelectUsers(int page = 1, int size = 10,
        string? search = null)
    {
        if (page < 1 || size < 1)
        {
            ModelState.AddModelError("", "страница и размер должны быть больше 1");

            return BadRequest(ModelState);
        }

        var users = dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            users = users.Where(u =>
                EF.Functions.Like(u.UserName, $"%{search}%") ||
                EF.Functions.Like(u.Email, $"%{search}%"));
        }

        return new SelectModel<UserModel>
        {
            Items = await users.OrderBy(u => u.UserName)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(g => mapper.Map<UserModel>(g))
                .ToListAsync(),
            TotalCount = await users.CountAsync()
        };
    }
    
    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser(UserWriteModel model)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<UserModel>> UpdateUser(Guid id, UserWriteModel model)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        throw new NotImplementedException();
    }
}