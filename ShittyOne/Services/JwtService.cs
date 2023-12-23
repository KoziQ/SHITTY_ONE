using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShittyOne.Models;

namespace ShittyOne.Services;

public interface IJwtService
{
    string GenerateRefresh();
    string GenerateToken(ClaimsIdentity identity);
    ClaimsIdentity GenerateClaimsIdentity(string email, Guid id, string securityStamp, IList<Claim> claims = null);
    ClaimsPrincipal PrincipalFromToken(string token);
}

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;

    public JwtService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id, string securityStamp,
        IList<Claim> claims = null)
    {
        var claimsIdentity = new ClaimsIdentity(new GenericIdentity(email, "Token"), new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim("ExpiresIn", _jwtOptions.EpiresIn)
        });

        foreach (var claim in claims ?? new List<Claim>()) claimsIdentity.AddClaim(claim);
        return claimsIdentity;
    }

    public string GenerateToken(ClaimsIdentity identity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jti = _jwtOptions.JtiGenerator();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(),
                ClaimValueTypes.Integer64)
        }.ToList();

        claims.Add(new Claim("CreationDatetime", DateTime.Now.ToString()));

        claims.AddRange(identity.Claims
            .Where(c => c.Type != JwtRegisteredClaimNames.Jti && c.Type != JwtRegisteredClaimNames.Iat).ToList());

        var expires = DateTime.Now.AddMinutes(int.Parse(_jwtOptions.EpiresIn));

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            expires: expires,
            notBefore: DateTime.Now,
            audience: _jwtOptions.Audience,
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.SecretKey))
                , SecurityAlgorithms.HmacSha256));

        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal PrincipalFromToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = false,
            ValidAudience = _jwtOptions.Issuer,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            SaveSigninToken = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        var principals = handler.ValidateToken(token, validationParams, out var securityToken);

        if ((securityToken as JwtSecurityToken) is null ||
            !(securityToken as JwtSecurityToken).Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase)) throw new SecurityTokenInvalidAlgorithmException();

        return principals;
    }

    public string GenerateRefresh()
    {
        var rnd = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(rnd);
        return Convert.ToBase64String(rnd);
    }


    private static long ToUnixEpochDate(DateTime date)
    {
        return (long)Math.Round((date.ToUniversalTime() -
                                 new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);
    }
}