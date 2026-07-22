using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_ObtenerPorUsuarioIdAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_ObtenerPorUsuarioIdAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task ObtenerPorUsuarioIdAsync_con_usuario_existente_retorna_estudiante_mapeado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var estudiante = new Estudiante("321456789", "CC", "Medicina", 8, usuarioId);
        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        // Act
        var resultado = await _sut.ObtenerPorUsuarioIdAsync(usuarioId);

        // Assert
        resultado.UsuarioId.Should().Be(usuarioId);
        resultado.ProgramaAcademico.Should().Be("Medicina");
    }

    [Fact]
    public async Task ObtenerPorUsuarioIdAsync_sin_estudiante_asociado_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        // Act
        var accion = async () => await _sut.ObtenerPorUsuarioIdAsync(Guid.NewGuid());

        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
    }
}