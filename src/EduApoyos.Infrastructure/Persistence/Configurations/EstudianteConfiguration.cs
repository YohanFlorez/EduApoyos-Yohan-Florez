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

        // UsuarioId es opciona porque el estudiante puede existir sin cuenta todavía
        builder.Property(e => e.UsuarioId).IsRequired(false);
        builder.Property(e => e.Activo)
               .HasDefaultValue(true);
       
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Estudiante>(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para agilizar consultas
        builder.HasIndex(e => e.NumeroDocumento)
            .IsUnique()
            .HasFilter("[Activo] = 1")
            .HasDatabaseName("IX_Estudiantes_NumeroDocumento");

        // Único, pero permite múltiples estudiantes con UsuarioId = NULL
        builder.HasIndex(e => e.UsuarioId).IsUnique();
        builder.HasIndex(e => e.Activo)
                .HasFilter("[Activo] = 1")
                .HasDatabaseName("IX_Estudiantes_Activo");

        builder.HasMany(e => e.Solicitudes)
            .WithOne()
            .HasForeignKey(s => s.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}