using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.Interfaces
{
    public interface IUsuarioLookupService
    {
        Task<bool> ExisteUsuarioConRolAsync(Guid usuarioId, RolUsuario rolEsperado, CancellationToken ct = default);
        Task<List<UsuarioPendienteResponse>> BuscarPendientesAsync(
         string? filtro,
         RolUsuario rol,
         IEnumerable<Guid> idsExcluidos,
         CancellationToken ct = default);
    }
}
