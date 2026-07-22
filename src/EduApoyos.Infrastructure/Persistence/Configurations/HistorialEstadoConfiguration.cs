using EduApoyos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

public class HistorialEstadoConfiguration : IEntityTypeConfiguration<HistorialEstado>
{
    public void Configure(EntityTypeBuilder<HistorialEstado> builder)
    {
        builder.ToTable("HistorialEstados");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        builder.Property(h => h.EstadoAnterior).HasConversion<int>();
        builder.Property(h => h.EstadoNuevo).HasConversion<int>();
        builder.Property(h => h.Observacion).HasMaxLength(500);
        //indice para agilizar consultas
        builder.HasIndex(h => h.SolicitudId);
    }
}
