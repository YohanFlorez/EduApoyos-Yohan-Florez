using EduApoyos.Application.Common;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.DTOs.Solicitudes;
using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.DTOs.Solicitudes.Response;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;

namespace EduApoyos.Application.Services;

public class SolicitudService : ISolicitudService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstadoSolicitudStrategy _estadoStrategy;

    public SolicitudService(IUnitOfWork unitOfWork, IEstadoSolicitudStrategy estadoStrategy)
    {
        _unitOfWork = unitOfWork;
        _estadoStrategy = estadoStrategy;
    }

    public async Task<SolicitudDetalleResponse> CrearAsync(
        CrearSolicitudRequest request, Guid usuarioActualId, RolUsuario rolActual, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(request.EstudianteId, ct)
            ?? throw new NotFoundException(nameof(Estudiante), request.EstudianteId);

        // Un estudiante solo puede crear solicitudes para sí mismo.
        // El asesor puede crear en nombre de cualquier estudiante.
        await VerificarPropiedadEstudianteAsync(estudiante.Id, usuarioActualId, rolActual, ct);

        if (!estudiante.Activo)
            throw new ConflictException("El estudiante está inactivo y no puede crear nuevas solicitudes.");

        var solicitud = new SolicitudApoyo(estudiante.Id, request.TipoApoyo, request.MontoSolicitado, request.Descripcion);

        await _unitOfWork.Solicitudes.AgregarAsync(solicitud, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return MapearDetalle(solicitud);
    }

    public async Task<PagedResponse<SolicitudListItemResponse>> ListarAsync(
        EstadoSolicitud? estado, TipoApoyo? tipoApoyo, DateTime? fechaDesde, DateTime? fechaHasta,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var filtro = new FiltroSolicitudes
        {
            Estado = estado,
            TipoApoyo = tipoApoyo,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
           // PageNumber = pageNumber,
           // PageSize = pageSize
        };

        var resultado = await _unitOfWork.Solicitudes.ListarAsync(filtro, pageNumber, pageSize, ct);
        return PagedResponse<SolicitudListItemResponse>.FromDomain(resultado, MapearListItem);
    }

    public async Task<SolicitudDetalleResponse> ObtenerDetalleAsync(Guid id, Guid usuarioActualId, RolUsuario rolActual, CancellationToken ct = default)
    {
        var solicitud = await _unitOfWork.Solicitudes.ObtenerPorIdConHistorialAsync(id, ct)
            ?? throw new NotFoundException(nameof(SolicitudApoyo), id);

        if (rolActual == RolUsuario.Estudiante)
        {
            var estudiante = await _unitOfWork.Estudiantes.ObtenerPorUsuarioIdAsync(usuarioActualId, ct);
            if (estudiante is null || !solicitud.PerteneceA(estudiante.Id))
                throw new ForbiddenAccessException("Solo puede consultar sus propias solicitudes.");
        }

        return MapearDetalle(solicitud);
    }


    public async Task<SolicitudDetalleResponse> CambiarEstadoAsync(Guid id, CambiarEstadoRequest request, Guid asesorId, CancellationToken ct = default)
    {
        var solicitud = await _unitOfWork.Solicitudes.ObtenerPorIdConHistorialAsync(id, ct)
            ?? throw new NotFoundException(nameof(SolicitudApoyo), id);

        solicitud.CambiarEstado(request.NuevoEstado, asesorId, _estadoStrategy, request.Observacion, asesorId);

        var nuevoRegistro = solicitud.Historial.Last();
        await _unitOfWork.HistorialEstado.AgregarHistorialAsync(nuevoRegistro, ct);

        await _unitOfWork.GuardarCambiosAsync(ct);

        return MapearDetalle(solicitud);
    }
    //public async Task<SolicitudDetalleResponse> CambiarEstadoAsync(Guid id, CambiarEstadoRequest request, Guid asesorId, CancellationToken ct = default)
    //{
    //    var solicitud = await _unitOfWork.Solicitudes.ObtenerPorIdConHistorialAsync(id, ct)
    //        ?? throw new NotFoundException(nameof(SolicitudApoyo), id);

    //    // La entidad valida la transición usando la estrategia (patrón Strategy) y genera el HistorialEstado.
    //    // Si la transición es inválida, SolicitudApoyo.CambiarEstado lanzará la excepción correspondiente,
    //    // que sube sin capturarse aquí para que el ExceptionHandlingMiddleware la traduzca al código HTTP correcto.
    //    solicitud.CambiarEstado(request.NuevoEstado, asesorId, _estadoStrategy, request.Observacion, asesorId);

    //    await _unitOfWork.GuardarCambiosAsync(ct);

    //    return MapearDetalle(solicitud);
    //}

    public async Task<PagedResponse<SolicitudListItemResponse>> ListarPorEstudianteAsync(
        Guid estudianteId, Guid usuarioActualId, RolUsuario rolActual, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        await VerificarPropiedadEstudianteAsync(estudianteId, usuarioActualId, rolActual, ct);

        var resultado = await _unitOfWork.Solicitudes.ListarPorEstudianteAsync(estudianteId, pageNumber, pageSize, ct);
        return PagedResponse<SolicitudListItemResponse>.FromDomain(resultado, MapearListItem);
    }

    /// <summary>
    /// Verifica que, cuando quien hace la petición es un Estudiante, solo pueda operar
    /// </summary>
    private async Task VerificarPropiedadEstudianteAsync(
        Guid estudianteId, Guid usuarioActualId, RolUsuario rolActual, CancellationToken ct)
    {
        if (rolActual != RolUsuario.Estudiante) return;

        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorUsuarioIdAsync(usuarioActualId, ct);
        if (estudiante is null || estudiante.Id != estudianteId)
            throw new ForbiddenAccessException("Solo puede consultar sus propias solicitudes.");
    }

    private static SolicitudListItemResponse MapearListItem(SolicitudApoyo s) => new SolicitudListItemResponse
    {
        Id = s.Id,
        EstudianteId = s.EstudianteId,
        TipoApoyo = s.TipoApoyo,
        MontoSolicitado = s.MontoSolicitado,
        Estado = s.Estado,
        FechaSolicitud = s.FechaSolicitud,
        FechaActualizacion = s.FechaActualizacion
    };


    private static SolicitudDetalleResponse MapearDetalle(SolicitudApoyo s) => new SolicitudDetalleResponse
    {
        Id = s.Id,
        EstudianteId = s.EstudianteId,
        TipoApoyo = s.TipoApoyo,
        MontoSolicitado = s.MontoSolicitado,
        Descripcion = s.Descripcion,
        Estado = s.Estado,
        FechaSolicitud = s.FechaSolicitud,
        FechaActualizacion = s.FechaActualizacion,
        AsesorId = s.AsesorId,

        Historial = s.Historial
            .OrderBy(h => h.FechaCambio)
            .Select(h => new HistorialEstadoResponse
            {
                EstadoAnterior = h.EstadoAnterior,
                EstadoNuevo = h.EstadoNuevo,
                FechaCambio = h.FechaCambio,
                UsuarioId = h.UsuarioId,
                Observacion = h.Observacion
            })
            .ToList()
    };


}