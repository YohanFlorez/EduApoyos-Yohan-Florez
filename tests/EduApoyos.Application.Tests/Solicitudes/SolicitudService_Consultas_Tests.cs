using EduApoyos.Application.DTOs.Solicitudes;
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

public class SolicitudService_Consultas_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly SolicitudService _sut;

    public SolicitudService_Consultas_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Solicitudes).Returns(_solicitudRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new SolicitudService(_unitOfWorkMock.Object, new FlujoEstandarEstadoSolicitudStrategy());
    }

    [Fact]
    public async Task ListarAsync_filtra_por_estado_y_devuelve_resultado_paginado()
    {
        // Arrange: datos simulados de dos solicitudes en distinto estado.
        var solicitudes = new List<SolicitudApoyo>
        {
            new(Guid.NewGuid(), TipoApoyo.Beca, 800_000m, "Solicitud 1"),
            new(Guid.NewGuid(), TipoApoyo.Credito, 1_200_000m, "Solicitud 2")
        };

        var pagina = new PagedResult<SolicitudApoyo>(solicitudes, totalCount: 2, pageNumber: 1, pageSize: 20);

        _solicitudRepoMock
                .Setup(r => r.ListarAsync(
                    It.Is<FiltroSolicitudes>(f => f.Estado == EstadoSolicitud.Pendiente),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagina);

        // Act
        var resultado = await _sut.ListarAsync(EstadoSolicitud.Pendiente, tipoApoyo: null, fechaDesde: null, fechaHasta: null, pageNumber: 1, pageSize: 20);

        // Assert
        resultado.TotalCount.Should().Be(2);
        resultado.Items.Should().HaveCount(2);
        resultado.Items.Should().Contain(i => i.TipoApoyo == TipoApoyo.Beca);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_estudiante_consultando_solicitud_ajena_lanza_ForbiddenAccessException()
    {
        // Arrange
        var otroEstudianteId = Guid.NewGuid();
        var solicitudAjena = new SolicitudApoyo(otroEstudianteId, TipoApoyo.Beca, 500_000m, "No es del usuario actual.");

        _solicitudRepoMock.Setup(r => r.ObtenerPorIdConHistorialAsync(solicitudAjena.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitudAjena);

        var usuarioActualId = Guid.NewGuid();
        var estudianteActual = new Estudiante("111222333", "CC", "Derecho", 3, usuarioActualId);

        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioActualId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudianteActual);

        // Act
        var accion = async () => await _sut.ObtenerDetalleAsync(solicitudAjena.Id, usuarioActualId, RolUsuario.Estudiante);

        // Assert
        await accion.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
