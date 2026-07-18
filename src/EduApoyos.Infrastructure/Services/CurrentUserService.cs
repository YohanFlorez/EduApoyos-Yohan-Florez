using System.Security.Claims;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EduApoyos.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UsuarioId
    {
        get
        {
            var valor = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public RolUsuario? Rol
    {
        get
        {
            var valor = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<RolUsuario>(valor, out var rol) ? rol : null;
        }
    }
}
