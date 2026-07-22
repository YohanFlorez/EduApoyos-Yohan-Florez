using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

// Ramas faltantes de CrearAsync: UsuarioId inválido / ya registrado / opcional
public class EstudianteService_CrearAsync_RamasUsuario_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_CrearAsync_RamasUsuario_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task CrearAsync_con_UsuarioId_que_no_existe_o_no_tiene_rol_Estudiante_lanza_ConflictException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _usuarioLookupMock
            .Setup(l => l.ExisteUsuarioConRolAsync(usuarioId, RolUsuario.Estudiante, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CrearEstudianteRequest
        {
            UsuarioId = usuarioId,
            NumeroDocumento = "900111222",
            TipoDocumento = "CC",
            ProgramaAcademico = "Ingeniería",
            Semestre = 2
        };

        // Act
        var accion = async () => await _sut.CrearAsync(request);

        // Assert
        await accion.Should().ThrowAsync<ConflictException>();
        _estudianteRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Estudiante>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_con_UsuarioId_que_ya_tiene_estudiante_registrado_lanza_ConflictException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _usuarioLookupMock
            .Setup(l => l.ExisteUsuarioConRolAsync(usuarioId, RolUsuario.Estudiante, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _estudianteRepoMock
            .Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Estudiante("800111222", "CC", "Ingeniería", 1, usuarioId));

        var request = new CrearEstudianteRequest
        {
            UsuarioId = usuarioId,
            NumeroDocumento = "900111222",
            TipoDocumento = "CC",
            ProgramaAcademico = "Ingeniería",
            Semestre = 2
        };

        // Act
        var accion = async () => await _sut.CrearAsync(request);

        // Assert
        await accion.Should().ThrowAsync<ConflictException>();
        _estudianteRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Estudiante>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_sin_UsuarioId_omite_validacion_de_usuario_y_persiste()
    {
        // Arrange: UsuarioId es opcional (nullable) — no debe llamar a _usuarioLookup
        _estudianteRepoMock
            .Setup(r => r.ObtenerPorNumeroDocumentoAsync("700111222", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        var request = new CrearEstudianteRequest
        {
            UsuarioId = null,
            NumeroDocumento = "700111222",
            TipoDocumento = "CC",
            ProgramaAcademico = "Derecho",
            Semestre = 1
        };

        // Act
        var resultado = await _sut.CrearAsync(request);

        // Assert
        resultado.NumeroDocumento.Should().Be("700111222");
        resultado.UsuarioId.Should().BeNull();
        _usuarioLookupMock.Verify(
            l => l.ExisteUsuarioConRolAsync(It.IsAny<Guid>(), It.IsAny<RolUsuario>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}