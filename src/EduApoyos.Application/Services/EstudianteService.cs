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
        // vincular todavía una cuenta de acceso (Opción A).
        if (request.UsuarioId.HasValue)
        {
            var usuarioValido = await _usuarioLookup.ExisteUsuarioConRolAsync(
                request.UsuarioId.Value, RolUsuario.Estudiante, ct);
            if (!usuarioValido)
                throw new AuthException("El UsuarioId indicado no existe o no tiene rol Estudiante.");
        }

        var existente = await _unitOfWork.Estudiantes.ObtenerPorNumeroDocumentoAsync(request.NumeroDocumento, ct);
        if (existente is not null)
            throw new AuthException("Ya existe un estudiante registrado con ese número de documento.");

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
        Semestre = e.Semestre
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

        estudiante.ActualizarDatosAcademicos(
            request.TipoDocumento,
            request.NumeroDocumento,
            request.ProgramaAcademico,
            request.Semestre);

        await _unitOfWork.GuardarCambiosAsync(ct);

        return Mapear(estudiante);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var estudiante = await _unitOfWork.Estudiantes.ObtenerPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Estudiante), id);

        var tieneSolicitudesActivas = await _unitOfWork.Solicitudes
            .TieneSolicitudesActivasAsync(id, ct);

        if (tieneSolicitudesActivas)
            throw new ConflictException(
                $"El estudiante '{id}' tiene solicitudes activas y no puede eliminarse.");

        _unitOfWork.Estudiantes.Eliminar(estudiante);
        await _unitOfWork.GuardarCambiosAsync(ct);
    }

}