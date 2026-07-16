using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.DTOs.Auth.Response
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiraEn { get; set; }

        public Guid UsuarioId { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; }
    }
}