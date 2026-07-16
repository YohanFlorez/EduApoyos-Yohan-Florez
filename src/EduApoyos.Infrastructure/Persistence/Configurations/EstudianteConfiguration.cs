using EduApoyos.Domain.Entities;
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

        builder.HasIndex(e => e.NumeroDocumento).IsUnique();
        builder.HasIndex(e => e.UsuarioId).IsUnique();

        builder.HasMany(e => e.Solicitudes)
            .WithOne()
            .HasForeignKey(s => s.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
