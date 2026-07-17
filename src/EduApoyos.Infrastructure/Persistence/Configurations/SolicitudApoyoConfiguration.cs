using EduApoyos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

public class SolicitudApoyoConfiguration : IEntityTypeConfiguration<SolicitudApoyo>
{
    public void Configure(EntityTypeBuilder<SolicitudApoyo> builder)
    {
        builder.ToTable("Solicitudes");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.MontoSolicitado).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Descripcion).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.TipoApoyo).HasConversion<int>();
        builder.Property(s => s.Estado).HasConversion<int>();

        builder.HasMany(s => s.Historial)
            .WithOne()
            .HasForeignKey(h => h.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        // indice no agrupado: acelera el listado/filtro del asesor por estado y fecha y el ejercicio de "pendientes con más de 5 días sin actualización".
        builder.HasIndex(s => new { s.Estado, s.FechaActualizacion })
            .HasDatabaseName("IX_Solicitudes_Estado_FechaActualizacion");

        builder.HasIndex(s => s.EstudianteId)
            .HasDatabaseName("IX_Solicitudes_EstudianteId");
    }
}
