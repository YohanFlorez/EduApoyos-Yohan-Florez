using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Interfaces;

public interface IPostRegistroHandler
{
    /// <summary>Rol para el cual este handler aplica.</summary>
    RolUsuario Rol { get; }

    /// <summary>
    /// Ejecuta la lógica adicional de registro. NO debe llamar a GuardarCambiosAsync/SaveChanges:
    /// eso lo controla el orquestador (AuthService) dentro de una única transacción.
    /// </summary>
    Task EjecutarAsync(Guid usuarioId, RegisterRequest request, CancellationToken ct);
}