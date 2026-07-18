using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.DTOs.Auth.Response;
using EduApoyos.Application.DTOs.Usuarios.Request;
using EduApoyos.Application.DTOs.Usuarios.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegistrarAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        Task<PerfilResponse> ObtenerPerfilAsync(Guid usuarioId, CancellationToken ct);
        Task<PerfilResponse> ActualizarPerfilAsync(Guid usuarioId, ActualizarPerfilRequest request, CancellationToken ct);
    }
}
