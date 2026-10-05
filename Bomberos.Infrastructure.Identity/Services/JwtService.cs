using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bomberos.Core.Domain.Settings;
using Bomberos.Infrastructure.Identity.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bomberos.Infrastructure.Identity.Services;

public interface IJwtService
{
    (string Token, DateTime Expira) Generate(ApplicationUser user, IEnumerable<string> roles);
}

public class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime Expira) Generate(ApplicationUser user, IEnumerable<string> roles)
    {
        // Claims con nombres literales (sub, name, role) para evitar problemas de mapeo entre componentes.
        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("name", user.UserName!),
            new("nombre", $"{user.Nombre} {user.Apellido}")
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var expira = DateTime.UtcNow.AddMinutes(_settings.DurationInMinutes);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
