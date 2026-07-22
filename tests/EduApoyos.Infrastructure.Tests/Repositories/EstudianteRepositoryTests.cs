using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Tests.Repositories;

public class EstudianteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly EduApoyosDbContext _context;
    private readonly EstudianteRepository _repository;

    public EstudianteRepositoryTests()
    {
        // SQLite en memoria: la conexión debe permanecer abierta durante toda la prueba,
        // porque al cerrarse se borra la base de datos.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EduApoyosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new EduApoyosDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new EstudianteRepository(_context);
    }

    [Fact]
    public async Task AgregarAsync_GuardaEstudianteCorrectamente()
    {
        // Arrange
        var estudiante = new Estudiante("1001234567", "CC", "Ingeniería de Sistemas", 5);

        // Act
        await _repository.AgregarAsync(estudiante);
        await _context.SaveChangesAsync();

        // Assert
        var guardado = await _context.Estudiantes.FindAsync(estudiante.Id);
        Assert.NotNull(guardado);
        Assert.Equal("1001234567", guardado!.NumeroDocumento);
        Assert.Equal("Ingeniería de Sistemas", guardado.ProgramaAcademico);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_RetornaNull_SiNoExiste()
    {
        // Act
        var resultado = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_RetornaEstudiante_SiExiste()
    {
        // Arrange
        var estudiante = new Estudiante("1002345678", "CC", "Derecho", 3);
        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObtenerPorIdAsync(estudiante.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(estudiante.Id, resultado!.Id);
    }

    [Fact]
    public async Task ObtenerPorNumeroDocumentoAsync_EncuentraElEstudianteCorrecto()
    {
        // Arrange
        var estudiante = new Estudiante("999888777", "TI", "Medicina", 8);
        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObtenerPorNumeroDocumentoAsync("999888777");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Medicina", resultado!.ProgramaAcademico);
    }

    [Fact]
    public async Task ListarAsync_RespetaPaginacionYOrdenAlfabetico()
    {
        // Arrange: 3 estudiantes en programas distintos, sin orden particular al insertarlos
        _context.Estudiantes.AddRange(
            new Estudiante("111", "CC", "Zootecnia", 1),
            new Estudiante("222", "CC", "Arquitectura", 2),
            new Estudiante("333", "CC", "Biología", 3)
        );
        await _context.SaveChangesAsync();

        // Act: pedimos página 1 con tamaño 2
        var resultado = await _repository.ListarAsync(pageNumber: 1, pageSize: 2);

        // Assert
        Assert.Equal(3, resultado.TotalCount);
        Assert.Equal(2, resultado.Items.Count);
        // Debe venir ordenado por ProgramaAcademico ascendente: Arquitectura, Biología...
        Assert.Equal("Arquitectura", resultado.Items.First().ProgramaAcademico);
    }

    [Fact]
    public async Task BuscarPorDocumentoAsync_FiltraPorCoincidenciaParcial()
    {
        // Arrange
        _context.Estudiantes.AddRange(
            new Estudiante("1020304050", "CC", "Física", 2),
            new Estudiante("1099887766", "CC", "Química", 4)
        );
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.BuscarPorDocumentoAsync("1020", CancellationToken.None);

        // Assert
        Assert.Single(resultado);
        Assert.Equal("1020304050", resultado.First().NumeroDocumento);
    }

    [Fact]
    public async Task BuscarPorDocumentoAsync_RetornaVacio_SiFiltroNoCoincide()
    {
        // Arrange
        _context.Estudiantes.Add(new Estudiante("1111111111", "CC", "Historia", 1));
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.BuscarPorDocumentoAsync("999999", CancellationToken.None);

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Eliminar_MarcaLaEntidadParaEliminacion()
    {
        // Arrange
        var estudiante = new Estudiante("555666777", "CC", "Economía", 6);
        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();

        // Act
        _repository.Eliminar(estudiante);
        await _context.SaveChangesAsync();

        // Assert
        var existe = await _context.Estudiantes.FindAsync(estudiante.Id);
        Assert.Null(existe);
    }

    [Fact]
    public async Task ObtenerTodosLosUsuarioIdsAsync_SoloRetornaEstudiantesSinUsuarioVinculado()
    {
        // Arrange: creamos un ApplicationUser real para no violar la FK
        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "test.usuario@eduapoyos.edu.co",
            Email = "test.usuario@eduapoyos.edu.co",
            NombreCompleto = "Usuario de Prueba",
            Rol = RolUsuario.Estudiante
        };
        _context.Users.Add(usuario);
        await _context.SaveChangesAsync();

        var sinUsuario = new Estudiante("777", "CC", "Filosofía", 2);
        var conUsuario = new Estudiante("888", "CC", "Enfermería", 3, usuarioId: usuario.Id);
        _context.Estudiantes.AddRange(sinUsuario, conUsuario);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObtenerTodosLosUsuarioIdsAsync();

        // Assert
        Assert.Single(resultado);
        Assert.Contains(sinUsuario.Id, resultado);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}