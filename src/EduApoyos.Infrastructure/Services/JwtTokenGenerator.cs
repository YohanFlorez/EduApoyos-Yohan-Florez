using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiraEn) GenerarToken(Guid usuarioId, string email, RolUsuario rol, string nombreCompleto)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_options.ExpiracionMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Name, nombreCompleto),
            new Claim(ClaimTypes.Role, rol.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiraEn,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
