using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShittyOne.Data;
using ShittyOne.Entities;
using ShittyOne.Models;
using ShittyOne.Services;
using System.Security.Claims;

namespace ShittyOne.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    public class TokenController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly AppDbContext _dbContext;
        private readonly JwtOptions _jwtOptions;

        public TokenController(IJwtService jwtService, IOptions<JwtOptions> options, UserManager<User> userManager, AppDbContext dbContext, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _jwtOptions = options.Value;
        }

        [HttpPost("")]
        public async Task<IActionResult> GetToken([FromBody] LoginModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Login);

            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return BadRequest(ModelState);
            }

            var valResult = await _userManager.CheckPasswordAsync(user, model.Password);

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

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>();

            foreach(var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var claim = await _roleManager.GetClaimsAsync(role);
                claims.AddRange(claim);
            }

            claims = claims.Distinct().ToList();

            var identity = _jwtService.GenerateClaimsIdentity(user.Email, user.Id, user.SecurityStamp, claims);

            var refresh = new UserRefresh
            {
                Token = _jwtService.GenerateRefresh(),
                User = user
            };

            _dbContext.Refreshes.Add(refresh);
            await _dbContext.SaveChangesAsync();

            var jwt = new TokenModel
            {
                Access = _jwtService.GenerateToken(identity),
                Refresh = refresh.Token
            };

            return Ok(jwt);
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> GetRefresh([FromBody] TokenModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var principals = _jwtService.PrincipalFromToken(model.Access);
            var user = await _dbContext.Users
                .Include(u =>u.Refreshes)
                .FirstOrDefaultAsync(u => u.Id.ToString() == principals.GetId());

            if(user == null)
            {
                return Forbid();
            }

            var refresh = user.Refreshes.FirstOrDefault(r => r.Token == model.Refresh);

            if(refresh == null || refresh.Date.Add(_jwtOptions.RrefreshLifetime) < DateTime.Now)
            {
                return Forbid();
            }

            var token = _jwtService.GenerateToken(principals.Identity as ClaimsIdentity);

            var newRef = new UserRefresh
            {
                Token = _jwtService.GenerateRefresh(),
                User = user
            };

            _dbContext.Refreshes.Add(newRef);
            _dbContext.Refreshes.Remove(refresh);
            await _dbContext.SaveChangesAsync();

            return Ok(new TokenModel
            {
                Access = token,
                Refresh = newRef.Token
            });
        }
    }
}
