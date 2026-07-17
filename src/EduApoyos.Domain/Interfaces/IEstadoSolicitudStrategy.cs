using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Interfaces;

/// <summary>
/// Patrón Strategy: encapsula la regla de qué transiciones de estado son válidas
/// para una solicitud de apoyo. Permite cambiar el flujo (p. ej. agregar un estado
/// "EnComite") sin tocar la entidad SolicitudApoyo ni los casos de uso.
/// </summary>
public interface IEstadoSolicitudStrategy
{
    bool EsTransicionValida(EstadoSolicitud actual, EstadoSolicitud nuevo);

    /// mensaje  de las transiciones permitidas desde un estado.
    string DescribirTransicionesPermitidas(EstadoSolicitud actual);
}
