using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_ActualizarYDesactivarAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_ActualizarYDesactivarAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Solicitudes).Returns(_solicitudRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    // ---------- ActualizarAsync ----------

    [Fact]
    public async Task ActualizarAsync_con_datos_validos_actualiza_y_persiste()
    {
        // Arrange
        var estudiante = new Estudiante("111111111", "CC", "Sistemas", 1);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var request = new ActualizarEstudianteRequest(
            TipoDocumento: "CC",
            NumeroDocumento: "111111111",
            ProgramaAcademico: "Sistemas Actualizado",
            Semestre: 3);

        // Act
        var resultado = await _sut.ActualizarAsync(estudiante.Id, request);

        // Assert
        resultado.ProgramaAcademico.Should().Be("Sistemas Actualizado");
        resultado.Semestre.Should().Be(3);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_con_id_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        var request = new ActualizarEstudianteRequest(
            TipoDocumento: "CC",
            NumeroDocumento: "000",
            ProgramaAcademico: "X",
            Semestre: 1);

        // Act
        var accion = async () => await _sut.ActualizarAsync(Guid.NewGuid(), request);

        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ActualizarAsync_cambiando_a_documento_de_otro_estudiante_lanza_ConflictException()
    {
        // Arrange
        var estudiante = new Estudiante("222222222", "CC", "Sistemas", 1);
        var otroEstudiante = new Estudiante("333333333", "CC", "Derecho", 2);

        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);
        _estudianteRepoMock.Setup(r => r.ObtenerPorNumeroDocumentoAsync("333333333", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otroEstudiante);

        var request = new ActualizarEstudianteRequest(
             TipoDocumento: "CC",
             NumeroDocumento: "333333333", // documento que ya pertenece a otro estudiante
             ProgramaAcademico: "Sistemas",
             Semestre: 2);

        // Act
        var accion = async () => await _sut.ActualizarAsync(estudiante.Id, request);

        // Assert
        await accion.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- DesactivarAsync ----------

    [Fact]
    public async Task DesactivarAsync_sin_solicitudes_activas_desactiva_correctamente()
    {
        // Arrange
        var estudiante = new Estudiante("444444444", "CC", "Sistemas", 1);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);
        _solicitudRepoMock.Setup(r => r.TieneSolicitudesActivasAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.DesactivarAsync(estudiante.Id);

        // Assert
        estudiante.Activo.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarAsync_con_id_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        // Act
        var accion = async () => await _sut.DesactivarAsync(Guid.NewGuid());

        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DesactivarAsync_con_solicitudes_activas_lanza_ConflictException()
    {
        // Arrange
        var estudiante = new Estudiante("555555555", "CC", "Sistemas", 1);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);
        _solicitudRepoMock.Setup(r => r.TieneSolicitudesActivasAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var accion = async () => await _sut.DesactivarAsync(estudiante.Id);

        // Assert
        await accion.Should().ThrowAsync<ConflictException>();
        estudiante.Activo.Should().BeTrue("no debió desactivarse si la excepción se lanzó antes");
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}