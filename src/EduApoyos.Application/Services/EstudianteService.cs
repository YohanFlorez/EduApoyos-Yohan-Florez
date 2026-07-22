using EduApoyos.Application.Common;
using EduApoyos.Application.DTOs.Estudiantes;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;

namespace EduApoyos.Application.Services;

public class EstudianteService : IEstudianteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioLookupService _usuarioLookup;

    public EstudianteService(IUnitOfWork unitOfWork, IUsuarioLookupService usuarioLookup)
    {
        _unitOfWork = unitOfWork;
        _usuarioLookup = usuarioLookup;
    }

    public async Task<EstudianteResponse> CrearAsync(CrearEstudianteRequest request, CancellationToken ct = default)
    {
        // UsuarioId es opcional: el asesor puede crear el estudiante sin
        if (request.UsuarioId.HasValue)
        {
            var usuarioValido = await _usuarioLookup.ExisteUsuarioConRolAsync(
                request.UsuarioId.Value, RolUsuario.Estudiante, ct);
            if (!usuarioValido)
                throw new ConflictException("El UsuarioId indicado no existe o no tiene rol Estudiante.");

            var estudianteConMismoUsuario = await _unitOfWork.Estudiantes.ObtenerPorUsuarioIdAsync(
                request.UsuarioId.Value, ct);
            if (estudianteConMismoUsuario is not null)
                throw new ConflictException("Ese usuario ya tiene un estudiante registrado.");
        }

        var existente = await _unitOfWork.Estudiantes.ObtenerPorNumeroDocumentoAsync(request.NumeroDocumento, ct);
        if (existente is not null)
            throw new ConflictException("Ya existe un estudiante registrado con ese número de documento.");

        var estudiante = new Estudiante(
            request.NumeroDocumento, request.TipoDocumento,
            request.ProgramaAcademico, request.Semestre, request.UsuarioId);

        await _unitOfWork.Estudiantes.AgregarAsync(estudiante, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);
        return Mapear(estudiante);
    }
    public async Task<EstudianteResponse> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Estudiante), id);
        return Mapear(estudiante);
    }

    public async Task<PagedResponse<EstudianteResponse>> ListarAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var resultado = await _unitOfWork.Estudiantes.ListarAsync(pageNumber, pageSize, ct);
        return PagedResponse<EstudianteResponse>.FromDomain(resultado, Mapear);
    }

    public async Task<List<UsuarioPendienteResponse>> ObtenerPendientesAsync(string? filtro, CancellationToken ct = default)
    {
        var idsConPerfil = await _unitOfWork.Estudiantes.ObtenerTodosLosUsuarioIdsAsync(ct);
        return await _usuarioLookup.BuscarPendientesAsync(filtro, RolUsuario.Estudiante, idsConPerfil, ct);
    }

    private static EstudianteResponse Mapear(Estudiante e) => new EstudianteResponse
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        NumeroDocumento = e.NumeroDocumento,
        TipoDocumento = e.TipoDocumento,
        ProgramaAcademico = e.ProgramaAcademico,
        Semestre = e.Semestre,
        Activo = e.Activo
    };

    public async Task<EstudianteResponse> ObtenerPorUsuarioIdAsync(
    Guid usuarioId,
    CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes
            .ObtenerPorUsuarioIdAsync(usuarioId, ct);

        if (estudiante is null)
            throw new NotFoundException(
                nameof(Estudiante),
                usuarioId);

        return Mapear(estudiante);
    }

    public async Task<EstudianteResponse> ActualizarAsync(
        Guid id, ActualizarEstudianteRequest request, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Estudiante), id);

       
        if (!string.Equals(estudiante.NumeroDocumento, request.NumeroDocumento, StringComparison.Ordinal))
        {
            var otroConEseDocumento = await _unitOfWork.Estudiantes
                .ObtenerPorNumeroDocumentoAsync(request.NumeroDocumento, ct);

            if (otroConEseDocumento is not null && otroConEseDocumento.Id != id)
                throw new ConflictException("Ya existe otro estudiante registrado con ese número de documento.");
        }

        estudiante.ActualizarDatosAcademicos(
            request.TipoDocumento,
            request.NumeroDocumento,
            request.ProgramaAcademico,
            request.Semestre);

        await _unitOfWork.GuardarCambiosAsync(ct);

        return Mapear(estudiante);
    }


    public async Task<List<EstudianteBusquedaResponse>> BuscarPorDocumentoAsync(
    string? filtro, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filtro))
            return new List<EstudianteBusquedaResponse>();

        var estudiantes = await _unitOfWork.Estudiantes.BuscarPorDocumentoAsync(filtro, ct);

        var usuarioIds = estudiantes
            .Where(e => e.UsuarioId.HasValue)
            .Select(e => e.UsuarioId!.Value);

        var nombresPorUsuarioId = await _usuarioLookup.ObtenerNombresPorUsuarioIdsAsync(usuarioIds, ct);

        return estudiantes.Select(e => new EstudianteBusquedaResponse
        {
            Id = e.Id,
            TipoDocumento = e.TipoDocumento,
            NumeroDocumento = e.NumeroDocumento,
            ProgramaAcademico = e.ProgramaAcademico,
            NombreCompleto = e.UsuarioId.HasValue && nombresPorUsuarioId.TryGetValue(e.UsuarioId.Value, out var n)
                ? n
                : null
        }).ToList();
    }


    /// <summary>
    /// Desactiva  al estudiante. No elimina el registro físicamente:
    /// </summary>
    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Estudiante), id);

        var tieneSolicitudesActivas = await _unitOfWork.Solicitudes
            .TieneSolicitudesActivasAsync(id, ct);

        if (tieneSolicitudesActivas)
            throw new ConflictException(
                $"El estudiante '{id}' tiene solicitudes activas y no puede eliminarse.");

        estudiante.Desactivar();

        await _unitOfWork.GuardarCambiosAsync(ct);
    }

    /// <summary>
    /// Reactiva a un estudiante previamente desactivado.
    /// </summary>
    public async Task ActivarAsync(Guid id, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Estudiante), id);

        estudiante.Activar();

        await _unitOfWork.GuardarCambiosAsync(ct);
    }

}