using EduApoyos.Application.DTOs.Solicitudes;
using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Solicitudes;

public class SolicitudService_CambiarEstadoAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly SolicitudService _sut;

    public SolicitudService_CambiarEstadoAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Solicitudes)
            .Returns(_solicitudRepoMock.Object);

        _sut = new SolicitudService(
            _unitOfWorkMock.Object,
            new FlujoEstandarEstadoSolicitudStrategy());
    }

    private static SolicitudApoyo CrearSolicitudPendiente() =>
        new(Guid.NewGuid(), TipoApoyo.Beca, 1_000_000m, "Solicitud de prueba.");

    [Fact]
    public async Task Cambiar_de_Pendiente_a_EnRevision_es_una_transicion_valida()
    {
        // Arrange
        var solicitud = CrearSolicitudPendiente();

        _solicitudRepoMock
            .Setup(r => r.ObtenerPorIdConHistorialAsync(
                solicitud.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var request = new CambiarEstadoRequest
        {
            NuevoEstado = EstadoSolicitud.EnRevision,
            Observacion = "Se inicia revisión de documentos."
        };

        // Act
        var resultado = await _sut.CambiarEstadoAsync(
            solicitud.Id,
            request,
            asesorId: Guid.NewGuid());

        // Assert
        resultado.Estado.Should().Be(EstadoSolicitud.EnRevision);
        resultado.Historial.Should().ContainSingle();
        resultado.Historial.Single().EstadoAnterior.Should().Be(EstadoSolicitud.Pendiente);
        resultado.Historial.Single().EstadoNuevo.Should().Be(EstadoSolicitud.EnRevision);

        _unitOfWorkMock.Verify(
            u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Cambiar_de_Pendiente_directo_a_Aprobada_es_una_transicion_invalida()
    {
        // Arrange
        var solicitud = CrearSolicitudPendiente();

        _solicitudRepoMock
            .Setup(r => r.ObtenerPorIdConHistorialAsync(
                solicitud.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var request = new CambiarEstadoRequest
        {
            NuevoEstado = EstadoSolicitud.Aprobada,
            Observacion = null
        };

        // Act
        var accion = async () =>
            await _sut.CambiarEstadoAsync(
                solicitud.Id,
                request,
                asesorId: Guid.NewGuid());

        // Assert
        await accion.Should()
            .ThrowAsync<Domain.Common.DomainException>();

        _unitOfWorkMock.Verify(
            u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task Cambiar_estado_de_una_solicitud_ya_Aprobada_es_invalido_por_ser_estado_final()
    {
        // Arrange
        var solicitud = CrearSolicitudPendiente();

        var strategy = new FlujoEstandarEstadoSolicitudStrategy();

        solicitud.CambiarEstado(
            EstadoSolicitud.EnRevision,
            Guid.NewGuid(),
            strategy);

        solicitud.CambiarEstado(
            EstadoSolicitud.Aprobada,
            Guid.NewGuid(),
            strategy);

        _solicitudRepoMock
            .Setup(r => r.ObtenerPorIdConHistorialAsync(
                solicitud.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var request = new CambiarEstadoRequest
        {
            NuevoEstado = EstadoSolicitud.Rechazada,
            Observacion = null
        };

        // Act
        var accion = async () =>
            await _sut.CambiarEstadoAsync(
                solicitud.Id,
                request,
                asesorId: Guid.NewGuid());

        // Assert
        await accion.Should()
            .ThrowAsync<Domain.Common.DomainException>();
    }


    [Fact]
    public async Task Cambiar_estado_de_solicitud_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _solicitudRepoMock
            .Setup(r => r.ObtenerPorIdConHistorialAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitudApoyo?)null);

        var request = new CambiarEstadoRequest
        {
            NuevoEstado = EstadoSolicitud.EnRevision,
            Observacion = null
        };

        // Act
        var accion = async () =>
            await _sut.CambiarEstadoAsync(
                Guid.NewGuid(),
                request,
                asesorId: Guid.NewGuid());

        // Assert
        await accion.Should()
            .ThrowAsync<NotFoundException>();
    }
}