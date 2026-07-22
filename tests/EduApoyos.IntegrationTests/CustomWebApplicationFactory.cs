using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Evita que Program.cs intente aplicar migraciones de SQL Server
        // (nvarchar(max), etc.) contra SQLite en memoria.
        Environment.SetEnvironmentVariable("SKIP_DB_MIGRATE", "true");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EduApoyosDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            _connection.Open();

            services.AddDbContext<EduApoyosDbContext>(options =>
                options.UseSqlite(_connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<EduApoyosDbContext>();
            db.Database.EnsureCreated(); // crea el esquema desde el modelo, sin SQL crudo de SQL Server

            // Sembramos roles y usuario Asesor de prueba manualmente,
            // igual que hace DbInitializer, pero sin MigrateAsync().
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var rol in Enum.GetNames<RolUsuario>())
            {
                if (!roleManager.RoleExistsAsync(rol).GetAwaiter().GetResult())
                    roleManager.CreateAsync(new IdentityRole<Guid>(rol)).GetAwaiter().GetResult();
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            const string emailAsesor = "asesor.demo@eduapoyos.edu.co";
            if (userManager.FindByEmailAsync(emailAsesor).GetAwaiter().GetResult() is null)
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
                var resultado = userManager.CreateAsync(asesor, "Demo1234*").GetAwaiter().GetResult();
                if (resultado.Succeeded)
                    userManager.AddToRoleAsync(asesor, RolUsuario.Asesor.ToString()).GetAwaiter().GetResult();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}