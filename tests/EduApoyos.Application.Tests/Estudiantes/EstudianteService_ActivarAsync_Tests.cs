using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_ActivarAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_ActivarAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task ActivarAsync_con_estudiante_existente_lo_reactiva_y_persiste()
    {
        // Arrange
        var estudiante = new Estudiante("444555666", "CC", "Sistemas", 2);
        estudiante.Desactivar(); // aseguramos que parte de estado inactivo

        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        // Act
        await _sut.ActivarAsync(estudiante.Id);

        // Assert
        estudiante.Activo.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivarAsync_con_id_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        // Act
        var accion = async () => await _sut.ActivarAsync(Guid.NewGuid());

        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}