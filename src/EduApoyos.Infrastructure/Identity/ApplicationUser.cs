using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string NombreCompleto { get; set; } = default!;
    public RolUsuario Rol { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
