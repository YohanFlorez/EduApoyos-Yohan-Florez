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
        var usuarioValido = await _usuarioLookup.ExisteUsuarioConRolAsync(request.UsuarioId, RolUsuario.Estudiante, ct);
        if (!usuarioValido)
            throw new AuthException("El UsuarioId indicado no existe o no tiene rol Estudiante.");

        var existente = await _unitOfWork.Estudiantes.ObtenerPorNumeroDocumentoAsync(request.NumeroDocumento, ct);
        if (existente is not null)
            throw new AuthException("Ya existe un estudiante registrado con ese número de documento.");

        var estudiante = new Estudiante(
            request.UsuarioId, request.NumeroDocumento, request.TipoDocumento,
            request.ProgramaAcademico, request.Semestre);

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

    private static EstudianteResponse Mapear(Estudiante e) => new EstudianteResponse
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        NumeroDocumento = e.NumeroDocumento,
        TipoDocumento = e.TipoDocumento,
        ProgramaAcademico = e.ProgramaAcademico,
        Semestre = e.Semestre
    };
}
