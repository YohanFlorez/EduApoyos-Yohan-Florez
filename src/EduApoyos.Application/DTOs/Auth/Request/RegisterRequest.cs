using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.DTOs.Auth.Request
{
    public class RegisterRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; }

        // Solo requeridos si Rol == Estudiante
        public string? NumeroDocumento { get; set; }

        public string? TipoDocumento { get; set; }

        public string? ProgramaAcademico { get; set; }

        public int? Semestre { get; set; }
    }
}