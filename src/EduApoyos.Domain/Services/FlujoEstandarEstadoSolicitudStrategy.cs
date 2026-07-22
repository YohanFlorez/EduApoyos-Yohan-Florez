using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Services;


/// implementación concreta (Strategy) del flujo estándar:Pendiente → EnRevision → Aprobada / Rechazada.
  // Rechazada y Aprobada son los estados finales.

public class FlujoEstandarEstadoSolicitudStrategy : Interfaces.IEstadoSolicitudStrategy
{
    private static readonly Dictionary<EstadoSolicitud, EstadoSolicitud[]> Transiciones = new()
    {
        [EstadoSolicitud.Pendiente] = new[] { EstadoSolicitud.EnRevision, EstadoSolicitud.Rechazada },
        [EstadoSolicitud.EnRevision] = new[] { EstadoSolicitud.Aprobada, EstadoSolicitud.Rechazada },
        [EstadoSolicitud.Aprobada] = Array.Empty<EstadoSolicitud>(),
        [EstadoSolicitud.Rechazada] = Array.Empty<EstadoSolicitud>()
    };

    public bool EsTransicionValida(EstadoSolicitud actual, EstadoSolicitud nuevo)
    {
        if (actual == nuevo) return false;
        return Transiciones.TryGetValue(actual, out var permitidos) && permitidos.Contains(nuevo);
    }

    public string DescribirTransicionesPermitidas(EstadoSolicitud actual)
    {
        var permitidos = Transiciones.TryGetValue(actual, out var valores) ? valores : Array.Empty<EstadoSolicitud>();
        return permitidos.Length == 0
            ? $"El estado '{actual}' es final y no admite cambios."
            : $"Desde '{actual}' solo se permite cambiar a: {string.Join(", ", permitidos)}.";
    }
}
