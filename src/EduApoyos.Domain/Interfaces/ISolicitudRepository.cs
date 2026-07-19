using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
namespace EduApoyos.Domain.Interfaces;

public class FiltroSolicitudes
{
    public EstadoSolicitud? Estado { get; set; }
    public TipoApoyo? TipoApoyo { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public interface ISolicitudRepository
{
    Task<SolicitudApoyo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<SolicitudApoyo?> ObtenerPorIdConHistorialAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SolicitudApoyo>> ListarAsync(
        FiltroSolicitudes filtro, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PagedResult<SolicitudApoyo>> ListarPorEstudianteAsync(Guid estudianteId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken ct = default);
    Task<bool> TieneSolicitudesActivasAsync(Guid estudianteId, CancellationToken ct = default);
}