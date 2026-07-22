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

public class SolicitudService_CrearAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<ISolicitudRepository> _solicitudRepoMock = new();
    private readonly SolicitudService _sut;

    public SolicitudService_CrearAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Solicitudes).Returns(_solicitudRepoMock.Object);

        _sut = new SolicitudService(_unitOfWorkMock.Object, new FlujoEstandarEstadoSolicitudStrategy());
    }

    [Fact]
    public async Task Crear_solicitud_valida_queda_en_estado_pendiente_y_se_persiste()
    {
        // Arrange
        var estudiante = new Estudiante("123456789", "CC", "Ingeniería de Sistemas", 5);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        // Caso 1
        var request = new CrearSolicitudRequest
        {
            EstudianteId = estudiante.Id,
            TipoApoyo = TipoApoyo.Beca,
            MontoSolicitado = 1_500_000m,
            Descripcion = "Apoyo por situación económica."
        };
        // Act
        var resultado = await _sut.CrearAsync(request, usuarioActualId: Guid.NewGuid(), rolActual: RolUsuario.Asesor);
        // Assert
        resultado.Estado.Should().Be(EstadoSolicitud.Pendiente);
        resultado.MontoSolicitado.Should().Be(1_500_000m);
        resultado.Historial.Should().BeEmpty("una solicitud recién creada aún no tiene cambios de estado");

        _solicitudRepoMock.Verify(r => r.AgregarAsync(It.IsAny<SolicitudApoyo>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_solicitud_para_estudiante_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        var request = new CrearSolicitudRequest
        {
            EstudianteId = Guid.NewGuid(),
            TipoApoyo = TipoApoyo.Credito,
            MontoSolicitado = 2_000_000m,
            Descripcion = "Crédito educativo."
        };
        // Act
        var accion = async () => await _sut.CrearAsync(request, usuarioActualId: Guid.NewGuid(), rolActual: RolUsuario.Asesor);
        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
        _solicitudRepoMock.Verify(r => r.AgregarAsync(It.IsAny<SolicitudApoyo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Crear_solicitud_con_monto_invalido_lanza_excepcion_de_dominio(decimal monto)
    {
        // Arrange
        var estudiante = new Estudiante("987654321", "CC", "Medicina", 2);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var request = new CrearSolicitudRequest
        {
            EstudianteId = estudiante.Id,
            TipoApoyo = TipoApoyo.Subsidio,
            MontoSolicitado = monto,
            Descripcion = "Subsidio de transporte."
        };
        // Act
        var accion = async () => await _sut.CrearAsync(request, usuarioActualId: Guid.NewGuid(), rolActual: RolUsuario.Asesor);
        // Assert
        await accion.Should().ThrowAsync<Domain.Common.DomainException>();
    }
}
