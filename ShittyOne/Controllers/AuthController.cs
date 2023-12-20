using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Models;
using ShittyOne.Services;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/auth")]
public class AuthController(
        IJwtService jwtService,
        IOptions<JwtOptions> options,
        UserManager<User> userManager,
        AppDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager)
    : Controller
{
    private readonly JwtOptions _jwtOptions = options.Value;

    /// <summary>
    ///     Получение токена
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<TokenModel>> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await userManager.FindByNameAsync(model.Login);

        if (user == null)
        {
            ModelState.AddModelError("", "Неверный логин или пароль");
            return BadRequest(ModelState);
        }

        var valResult = await userManager.CheckPasswordAsync(user, model.Password);

        if (!valResult)
        {
            ModelState.AddModelError("", "Неверный логин или пароль");
            return BadRequest(ModelState);
        }

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError("", "Подтвердите Email");
            return BadRequest(ModelState);
        }

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>();

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            var claim = await roleManager.GetClaimsAsync(role!);
            claims.AddRange(claim);
        }

        claims = claims.Distinct().ToList();

        var identity = jwtService.GenerateClaimsIdentity(user!.Email!, user.Id, user!.SecurityStamp!, claims);

        var refresh = new UserRefresh
        {
            Token = jwtService.GenerateRefresh(),
            User = user
        };

        dbContext.Refreshes.Add(refresh);
        await dbContext.SaveChangesAsync();

        var jwt = new TokenModel
        {
            AccessToken = jwtService.GenerateToken(identity),
            RefreshToken = refresh.Token
        };

        return Ok(jwt);
    }

    /// <summary>
    ///     Рефреш токена
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenModel>> Refresh([FromBody] TokenModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var principals = jwtService.PrincipalFromToken(model.AccessToken);
        var user = await dbContext.Users
            .Include(u => u.Refreshes)
            .FirstOrDefaultAsync(u => u.Id.ToString() == principals.GetId());

        if (user == null) return Unauthorized();

        var refresh = user.Refreshes.FirstOrDefault(r => r.Token == model.RefreshToken);

        if (refresh == null || refresh.Date.Add(_jwtOptions.RrefreshLifetime) < DateTime.Now) return Forbid();

        var token = jwtService.GenerateToken((ClaimsIdentity)principals!.Identity!);

        var newRef = new UserRefresh
        {
            Token = jwtService.GenerateRefresh(),
            User = user
        };

        dbContext.Refreshes.Add(newRef);
        dbContext.Refreshes.Remove(refresh);
        await dbContext.SaveChangesAsync();

        return Ok(new TokenModel
        {
            AccessToken = token,
            RefreshToken = newRef.Token
        });
    }
}