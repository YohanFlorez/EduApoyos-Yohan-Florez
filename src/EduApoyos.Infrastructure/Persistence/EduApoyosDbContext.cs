using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence;

public class EduApoyosDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public EduApoyosDbContext(DbContextOptions<EduApoyosDbContext> options) : base(options)
    {
    }

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<SolicitudApoyo> Solicitudes => Set<SolicitudApoyo>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(EduApoyosDbContext).Assembly);

        builder.Entity<ApplicationUser>().ToTable("Usuarios");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UsuarioRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UsuarioClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UsuarioLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UsuarioTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RolClaims");
    }
}
