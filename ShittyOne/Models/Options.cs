using Microsoft.IdentityModel.Tokens;

namespace ShittyOne.Models;

public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public SigningCredentials Credentials { get; set; }
    public DateTime Expiration => IssuedAt.Add(ValidFor);
    public DateTime NotBefore => DateTime.Now;
    public DateTime IssuedAt => DateTime.Now;
    public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(120);
    public string EpiresIn { get; set; }
    public Func<string> JtiGenerator => () => Guid.NewGuid().ToString();
    public string SecretKey { get; set; }
    public TimeSpan RrefreshLifetime { get; set; }
}

public class ImapEmailOptions
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
}