using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_BuscarPorDocumentoAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_BuscarPorDocumentoAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BuscarPorDocumentoAsync_con_filtro_vacio_o_nulo_retorna_lista_vacia_sin_consultar_repositorio(string? filtro)
    {
        // Act
        var resultado = await _sut.BuscarPorDocumentoAsync(filtro);

        // Assert
        resultado.Should().BeEmpty();
        _estudianteRepoMock.Verify(
            r => r.BuscarPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BuscarPorDocumentoAsync_con_estudiante_con_usuario_asociado_incluye_nombre_completo()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var estudiante = new Estudiante("999888777", "CC", "Sistemas", 4, usuarioId);

        _estudianteRepoMock
            .Setup(r => r.BuscarPorDocumentoAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Estudiante> { estudiante });

        _usuarioLookupMock
            .Setup(l => l.ObtenerNombresPorUsuarioIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(usuarioId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [usuarioId] = "Ana Martínez" });

        // Act
        var resultado = await _sut.BuscarPorDocumentoAsync("999");

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].NombreCompleto.Should().Be("Ana Martínez");
    }

    [Fact]
    public async Task BuscarPorDocumentoAsync_con_estudiante_sin_usuario_asociado_nombre_es_null()
    {
        // Arrange: estudiante creado sin UsuarioId (registrado directamente por el asesor)
        var estudiante = new Estudiante("555444333", "CC", "Contaduría", 2);

        _estudianteRepoMock
            .Setup(r => r.BuscarPorDocumentoAsync("555", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Estudiante> { estudiante });

        _usuarioLookupMock
            .Setup(l => l.ObtenerNombresPorUsuarioIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>());

        // Act
        var resultado = await _sut.BuscarPorDocumentoAsync("555");

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].NombreCompleto.Should().BeNull();
    }
}