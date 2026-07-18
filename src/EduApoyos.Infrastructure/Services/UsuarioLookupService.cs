using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Services;

public class UsuarioLookupService : IUsuarioLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsuarioLookupService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> ExisteUsuarioConRolAsync(Guid usuarioId, RolUsuario rolEsperado, CancellationToken ct = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        return usuario is not null && usuario.Rol == rolEsperado;
    }

    
    public async Task<List<UsuarioPendienteResponse>> BuscarPendientesAsync(
        string? filtro,
        RolUsuario rol,
        IEnumerable<Guid> idsExcluidos,
        CancellationToken ct = default)
    {
        var query = _userManager.Users
            .Where(u => u.Rol == rol && !idsExcluidos.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(u =>
                u.NombreCompleto.Contains(filtro) ||
                u.Email!.Contains(filtro));
        }

        return await query
            .OrderBy(u => u.NombreCompleto)
            .Take(20)
            .Select(u => new UsuarioPendienteResponse
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email!
            })
            .ToListAsync(ct);
    }

    // Infrastructure/Services/UsuarioLookupService.cs — agregar
    public async Task<Dictionary<Guid, string>> ObtenerNombresPorUsuarioIdsAsync(
        IEnumerable<Guid> usuarioIds, CancellationToken ct)
    {
        var ids = usuarioIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        return await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NombreCompleto, ct);
    }
}
