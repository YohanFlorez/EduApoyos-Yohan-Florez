using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Estudiantes;

public class EstudianteService_ListarAsync_Tests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEstudianteRepository> _estudianteRepoMock = new();
    private readonly Mock<IUsuarioLookupService> _usuarioLookupMock = new();
    private readonly EstudianteService _sut;

    public EstudianteService_ListarAsync_Tests()
    {
        _unitOfWorkMock.SetupGet(u => u.Estudiantes).Returns(_estudianteRepoMock.Object);
        _sut = new EstudianteService(_unitOfWorkMock.Object, _usuarioLookupMock.Object);
    }

    [Fact]
    public async Task ListarAsync_delega_en_repositorio_y_mapea_resultado_paginado()
    {
        // Arrange
        var estudiantes = new List<Estudiante>
        {
            new("111000111", "CC", "Sistemas", 1),
            new("222000222", "CC", "Derecho", 3)
        };
        var pagedResult = new PagedResult<Estudiante>(estudiantes, totalCount: 2, pageNumber: 1, pageSize: 10);

        _estudianteRepoMock
            .Setup(r => r.ListarAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var resultado = await _sut.ListarAsync(1, 10);

        // Assert
        resultado.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
        _estudianteRepoMock.Verify(r => r.ListarAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}