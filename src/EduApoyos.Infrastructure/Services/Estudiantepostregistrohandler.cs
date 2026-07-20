using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;

namespace EduApoyos.Infrastructure.Services.PostRegistro;

/// <summary>
/// Crea la entidad de dominio Estudiante asociada al usuario recién registrado.
/// </summary>
public class EstudiantePostRegistroHandler : IPostRegistroHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public RolUsuario Rol => RolUsuario.Estudiante;

    public EstudiantePostRegistroHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(Guid usuarioId, RegisterRequest request, CancellationToken ct)
    {
        var estudiante = new Estudiante(
            request.NumeroDocumento!,
            request.TipoDocumento!,
            request.ProgramaAcademico!,
            request.Semestre!.Value,
            usuarioId);

        await _unitOfWork.Estudiantes.AgregarAsync(estudiante, ct);
    }
}