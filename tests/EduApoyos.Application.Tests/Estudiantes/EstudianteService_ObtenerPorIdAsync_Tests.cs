using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_ObtenerPorIdAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_ObtenerPorIdAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_con_id_existente_retorna_estudiante_mapeado()
    {
        // Arrange
        var estudiante = new Estudiante("123456789", "CC", "Sistemas", 5);
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        // Act
        var resultado = await _sut.ObtenerPorIdAsync(estudiante.Id);

        // Assert
        resultado.Id.Should().Be(estudiante.Id);
        resultado.ProgramaAcademico.Should().Be("Sistemas");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_con_id_inexistente_lanza_NotFoundException()
    {
        // Arrange
        _estudianteRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Estudiante?)null);

        // Act
        var accion = async () => await _sut.ObtenerPorIdAsync(Guid.NewGuid());

        // Assert
        await accion.Should().ThrowAsync<NotFoundException>();
    }
}