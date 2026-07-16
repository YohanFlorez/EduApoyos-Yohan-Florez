using EduApoyos.Domain.Common;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;

namespace EduApoyos.Domain.Entities;


public class SolicitudApoyo
{
    public Guid Id { get; private set; }
    public Guid EstudianteId { get; private set; }
    public TipoApoyo TipoApoyo { get; private set; }
    public decimal MontoSolicitado { get; private set; }
    public string Descripcion { get; private set; } = default!;
    public EstadoSolicitud Estado { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public DateTime FechaActualizacion { get; private set; }
    public Guid? AsesorId { get; private set; }

    private readonly List<HistorialEstado> _historial = new();
    public IReadOnlyCollection<HistorialEstado> Historial => _historial.AsReadOnly();

    protected SolicitudApoyo()
    {
        
    }

    public SolicitudApoyo(Guid estudianteId, TipoApoyo tipoApoyo, decimal montoSolicitado, string descripcion)
    {
        if (estudianteId == Guid.Empty)
            throw new DomainException("La solicitud debe estar asociada a un estudiante válido.");
        if (montoSolicitado <= 0)
            throw new DomainException("El monto solicitado debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción de la solicitud es obligatoria.");

        Id = Guid.NewGuid();
        EstudianteId = estudianteId;
        TipoApoyo = tipoApoyo;
        MontoSolicitado = montoSolicitado;
        Descripcion = descripcion;
        Estado = EstadoSolicitud.Pendiente;
        FechaSolicitud = DateTime.UtcNow;
        FechaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el estado de la solicitud validando la transición con la estrategia
    /// inyectada y deja registro en el historial. Si se asigna revisor, guarda el AsesorId.
    /// </summary>
    public void CambiarEstado(EstadoSolicitud nuevoEstado, Guid usuarioId,
        IEstadoSolicitudStrategy estrategia, string? observacion = null, Guid? asesorId = null)
    {
        if (!estrategia.EsTransicionValida(Estado, nuevoEstado))
            throw new DomainException(estrategia.DescribirTransicionesPermitidas(Estado));

        var estadoAnterior = Estado;
        Estado = nuevoEstado;
        FechaActualizacion = DateTime.UtcNow;
        if (asesorId.HasValue) AsesorId = asesorId;

        _historial.Add(new HistorialEstado(Id, estadoAnterior, nuevoEstado, usuarioId, observacion));
    }

    /// el estudiante solo ve sus propias solicitudes
    public bool PerteneceA(Guid estudianteId) => EstudianteId == estudianteId;
}
