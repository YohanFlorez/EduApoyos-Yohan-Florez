using EduApoyos.Application.Common;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.DTOs.Solicitudes.Response;
using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.Interfaces
{
    public interface ISolicitudService
    {
        Task<SolicitudDetalleResponse> CrearAsync(CrearSolicitudRequest request, Guid usuarioActualId, CancellationToken ct = default);

        Task<PagedResponse<SolicitudListItemResponse>> ListarAsync(
            EstadoSolicitud? estado, TipoApoyo? tipoApoyo, DateTime? fechaDesde, DateTime? fechaHasta,
            int pageNumber, int pageSize, CancellationToken ct = default);

        /// obtiene el detalle validando que, si quien consulta es Estudiante, la solicitud le pertenezca.
        Task<SolicitudDetalleResponse> ObtenerDetalleAsync(Guid id, Guid usuarioActualId, RolUsuario rolActual, CancellationToken ct = default);

        Task<SolicitudDetalleResponse> CambiarEstadoAsync(Guid id, CambiarEstadoRequest request, Guid asesorId, CancellationToken ct = default);

        Task<PagedResponse<SolicitudListItemResponse>> ListarPorEstudianteAsync(
            Guid estudianteId, Guid usuarioActualId, RolUsuario rolActual, int pageNumber, int pageSize, CancellationToken ct = default);

  
    }
}
