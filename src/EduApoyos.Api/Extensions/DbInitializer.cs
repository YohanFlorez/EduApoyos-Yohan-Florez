using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Api.Extensions;

/// Aplica migraciones pendientes y siembra roles y un usuario Asesor de prueba para poder probar el Api .
public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduApoyosDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var rol in Enum.GetNames<RolUsuario>())
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole<Guid>(rol));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string emailAsesor = "asesor.demo@eduapoyos.edu.co";
        if (await userManager.FindByEmailAsync(emailAsesor) is null)
        {
            var asesor = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = emailAsesor,
                Email = emailAsesor,
                NombreCompleto = "Asesor Demo",
                Rol = RolUsuario.Asesor,
                EmailConfirmed = true
            };

            // Contraseña de prueba SOLO para entorno local — cambiar/eliminar antes de producción.
            var resultado = await userManager.CreateAsync(asesor, "Demo1234*");
            if (resultado.Succeeded)
                await userManager.AddToRoleAsync(asesor, RolUsuario.Asesor.ToString());
        }
    }
}
