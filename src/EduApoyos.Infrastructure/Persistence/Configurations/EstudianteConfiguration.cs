using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

public class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(30);
        builder.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ProgramaAcademico).IsRequired().HasMaxLength(150);

        // UsuarioId es opcional: el estudiante puede existir sin cuenta todavía
        builder.Property(e => e.UsuarioId).IsRequired(false);

        // NUEVO: esto es lo que faltaba. Sin esto, UsuarioId era solo una
        // columna suelta, sin restricción real de integridad hacia Usuarios.
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Estudiante>(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para agilizar consultas
        builder.HasIndex(e => e.NumeroDocumento).IsUnique();

        // Único, pero permite múltiples estudiantes con UsuarioId = NULL
        builder.HasIndex(e => e.UsuarioId).IsUnique();

        builder.HasMany(e => e.Solicitudes)
            .WithOne()
            .HasForeignKey(s => s.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}