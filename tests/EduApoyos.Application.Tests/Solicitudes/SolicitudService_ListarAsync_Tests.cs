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

public class SolicitudService_ListarAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly SolicitudService _sut;

    public SolicitudService_ListarAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Solicitudes).Returns(_solicitudRepoMock.Object);

        _sut = new SolicitudService(
            _unitOfWorkMock.Object,
            new FlujoEstandarEstadoSolicitudStrategy());
    }

    [Fact]
    public async Task Listar_solicitudes_filtradas_por_estado_devuelve_solo_las_que_coinciden()
    {
        // Arrange: datos simulados, como si el repo ya hubiera filtrado por Pendiente
        var solicitudesSimuladas = new List<SolicitudApoyo>
        {
            new(Guid.NewGuid(), TipoApoyo.Beca, 500_000m, "Pendiente 1"),
            new(Guid.NewGuid(), TipoApoyo.Credito, 800_000m, "Pendiente 2"),
        };

        var resultadoDominio = new PagedResult<SolicitudApoyo>(
            items: solicitudesSimuladas,
            totalCount: solicitudesSimuladas.Count,
            pageNumber: 1,
            pageSize: 10);

        _solicitudRepoMock
            .Setup(r => r.ListarAsync(
                It.IsAny<FiltroSolicitudes>(),
                1, 10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoDominio);

        // Act
        var resultado = await _sut.ListarAsync(
            estado: EstadoSolicitud.Pendiente,
            tipoApoyo: null,
            fechaDesde: null,
            fechaHasta: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        resultado.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
        resultado.TotalPages.Should().Be(1);

        _solicitudRepoMock.Verify(
            r => r.ListarAsync(
                It.Is<FiltroSolicitudes>(f => f.Estado == EstadoSolicitud.Pendiente),
                1, 10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Listar_solicitudes_sin_resultados_devuelve_lista_vacia()
    {
        // Arrange
        var resultadoDominio = new PagedResult<SolicitudApoyo>(
            items: new List<SolicitudApoyo>(),
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10);

        _solicitudRepoMock
            .Setup(r => r.ListarAsync(
                It.IsAny<FiltroSolicitudes>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoDominio);

        // Act
        var resultado = await _sut.ListarAsync(
            estado: EstadoSolicitud.Rechazada,
            tipoApoyo: null, fechaDesde: null, fechaHasta: null,
            pageNumber: 1, pageSize: 10);

        // Assert
        resultado.Items.Should().BeEmpty();
        resultado.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Listar_solicitudes_con_filtro_por_tipo_de_apoyo_pasa_el_filtro_correcto_al_repositorio()
    {
        // Arrange
        var resultadoDominio = new PagedResult<SolicitudApoyo>(
            items: new List<SolicitudApoyo>(),
            totalCount: 0,
            pageNumber: 2,
            pageSize: 5);

        _solicitudRepoMock
            .Setup(r => r.ListarAsync(
                It.IsAny<FiltroSolicitudes>(), 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoDominio);

        // Act
        await _sut.ListarAsync(
            estado: null,
            tipoApoyo: TipoApoyo.Credito,
            fechaDesde: null,
            fechaHasta: null,
            pageNumber: 2,
            pageSize: 5);

        // Assert: verifica que el service arma bien el filtro y respeta paginación
        _solicitudRepoMock.Verify(
            r => r.ListarAsync(
                It.Is<FiltroSolicitudes>(f =>
                    f.Estado == null &&
                    f.TipoApoyo == TipoApoyo.Credito),
                2, 5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}