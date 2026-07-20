using EduApoyos.Application.DTOs.Estudiantes;
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

public class EstudianteService_CrearAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_CrearAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes)
            .Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(
            _unitOfWorkMock.Object,
            _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task Crear_estudiante_con_usuario_valido_y_documento_unico_se_persiste()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _usuarioLookupMock
            .Setup(l => l.ExisteUsuarioConRolAsync(
                usuarioId,
                RolUsuario.Estudiante,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _estudianteRepoMock
            .Setup(r => r.ObtenerPorNumeroDocumentoAsync(
                "100200300",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        var request = new CrearEstudianteRequest
        {
            UsuarioId = usuarioId,
            NumeroDocumento = "100200300",
            TipoDocumento = "CC",
            ProgramaAcademico = "Contaduría",
            Semestre = 4
        };

        // Act
        var resultado = await _sut.CrearAsync(request);

        // Assert
        resultado.NumeroDocumento.Should().Be("100200300");
        _estudianteRepoMock.Verify(
            r => r.AgregarAsync(
                It.IsAny<Estudiante>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Crear_estudiante_con_documento_duplicado_lanza_AuthException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _usuarioLookupMock
            .Setup(l => l.ExisteUsuarioConRolAsync(
                usuarioId,
                RolUsuario.Estudiante,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _estudianteRepoMock
            .Setup(r => r.ObtenerPorNumeroDocumentoAsync(
                "100200300",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Estudiante(
                    "100200300",
                    "CC",
                    "Otra carrera",
                    1));

        var request = new CrearEstudianteRequest
        {
            UsuarioId = usuarioId,
            NumeroDocumento = "100200300",
            TipoDocumento = "CC",
            ProgramaAcademico = "Contaduría",
            Semestre = 4
        };

        // Act
        var accion = async () => await _sut.CrearAsync(request);

        // Assert
        await accion.Should()
            .ThrowAsync<AuthException>();
    }
}