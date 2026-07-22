using EduApoyos.Application.Common;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Solicitudes;

public class SolicitudService_ListarPorEstudianteAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly SolicitudService _sut;

    public SolicitudService_ListarPorEstudianteAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Solicitudes).Returns(_solicitudRepoMock.Object);
        _sut = new SolicitudService(_unitOfWorkMock.Object, new FlujoEstandarEstadoSolicitudStrategy());
    }

    [Fact]
    public async Task Asesor_puede_listar_solicitudes_de_cualquier_estudiante()
    {
        // Arrange
        var estudianteId = Guid.NewGuid();
        var pagina = new PagedResult<SolicitudApoyo>(
            new List<SolicitudApoyo>(), totalCount: 0, pageNumber: 1, pageSize: 20);

        _solicitudRepoMock.Setup(r => r.ListarPorEstudianteAsync(
                estudianteId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagina);

        // Act
        var resultado = await _sut.ListarPorEstudianteAsync(
            estudianteId, usuarioActualId: Guid.NewGuid(), rolActual: RolUsuario.Asesor, pageNumber: 1, pageSize: 20);

        // Assert
        resultado.TotalCount.Should().Be(0);
        _estudianteRepoMock.Verify(
            r => r.ObtenerPorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never,
            "el asesor no requiere validación de propiedad");
    }

    [Fact]
    public async Task Estudiante_puede_listar_sus_propias_solicitudes()
    {
        // Arrange
        var usuarioActualId = Guid.NewGuid();
        var estudiante = new Estudiante("777888999", "CC", "Sistemas", 4, usuarioActualId);

        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioActualId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var pagina = new PagedResult<SolicitudApoyo>(
            new List<SolicitudApoyo>(), totalCount: 0, pageNumber: 1, pageSize: 20);
        _solicitudRepoMock.Setup(r => r.ListarPorEstudianteAsync(
                estudiante.Id, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagina);

        // Act
        var resultado = await _sut.ListarPorEstudianteAsync(
            estudiante.Id, usuarioActualId, RolUsuario.Estudiante, pageNumber: 1, pageSize: 20);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task Estudiante_intentando_listar_solicitudes_de_otro_lanza_ForbiddenAccessException()
    {
        // Arrange
        var usuarioActualId = Guid.NewGuid();
        var estudianteActual = new Estudiante("111000111", "CC", "Sistemas", 4, usuarioActualId);
        var estudianteAjenoId = Guid.NewGuid(); // distinto al del usuario autenticado

        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioActualId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudianteActual);

        // Act
        var accion = async () => await _sut.ListarPorEstudianteAsync(
            estudianteAjenoId, usuarioActualId, RolUsuario.Estudiante, pageNumber: 1, pageSize: 20);

        // Assert
        await accion.Should().ThrowAsync<ForbiddenAccessException>();
        _solicitudRepoMock.Verify(r => r.ListarPorEstudianteAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Estudiante_sin_perfil_asociado_lanza_ForbiddenAccessException()
    {
        // Arrange
        var usuarioActualId = Guid.NewGuid();
        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioActualId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        // Act
        var accion = async () => await _sut.ListarPorEstudianteAsync(
            Guid.NewGuid(), usuarioActualId, RolUsuario.Estudiante, pageNumber: 1, pageSize: 20);

        // Assert
        await accion.Should().ThrowAsync<ForbiddenAccessException>();
    }
}