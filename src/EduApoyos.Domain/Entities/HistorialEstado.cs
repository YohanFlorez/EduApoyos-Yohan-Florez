using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

public class HistorialEstado
{
    public Guid Id { get; private set; }
    public Guid SolicitudId { get; private set; }
    public EstadoSolicitud EstadoAnterior { get; private set; }
    public EstadoSolicitud EstadoNuevo { get; private set; }
    public DateTime FechaCambio { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string? Observacion { get; private set; }

    protected HistorialEstado()
    {
        // Requerido por EF Core
    }

    internal HistorialEstado(Guid solicitudId, EstadoSolicitud estadoAnterior,
        EstadoSolicitud estadoNuevo, Guid usuarioId, string? observacion)
    {
        Id = Guid.NewGuid();
        SolicitudId = solicitudId;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        FechaCambio = DateTime.UtcNow;
        UsuarioId = usuarioId;
        Observacion = observacion;
    }
}
