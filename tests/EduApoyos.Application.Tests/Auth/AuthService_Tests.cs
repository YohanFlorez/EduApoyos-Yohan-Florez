using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.DTOs.Usuarios.Request;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace EduApoyos.Application.Tests.Auth;

public class AuthService_Tests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AuthService _sut;

    public AuthService_Tests()
    {
        _userManagerMock = CrearUserManagerMock();

        // Sin handlers de post-registro para los casos base.
        _sut = new AuthService(
            _userManagerMock.Object,
            _jwtGeneratorMock.Object,
            _unitOfWorkMock.Object,
            Enumerable.Empty<IPostRegistroHandler>());
    }

    private static Mock<UserManager<ApplicationUser>> CrearUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // ---------- RegistrarAsync ----------

    [Fact]
    public async Task RegistrarAsync_con_correo_nuevo_crea_usuario_y_devuelve_token()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _jwtGeneratorMock.Setup(j => j.GenerarToken(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<RolUsuario>(), It.IsAny<string>()))
            .Returns(("token-fake", DateTime.UtcNow.AddHours(1)));

        var request = new RegisterRequest
        {
            Email = "nuevo@correo.com",
            Password = "Clave123!",
            NombreCompleto = "Juan Pérez",
            Rol = RolUsuario.Estudiante
        };

        // Act
        var resultado = await _sut.RegistrarAsync(request);

        // Assert
        resultado.Token.Should().Be("token-fake");
        resultado.Email.Should().Be("nuevo@correo.com");
        resultado.Rol.Should().Be(RolUsuario.Estudiante);

        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.ConfirmarTransaccionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RevertirTransaccionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_con_correo_ya_existente_lanza_AuthException()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), Email = "repetido@correo.com" });

        var request = new RegisterRequest
        {
            Email = "repetido@correo.com",
            Password = "Clave123!",
            NombreCompleto = "María Gómez",
            Rol = RolUsuario.Estudiante
        };

        // Act
        var accion = async () => await _sut.RegistrarAsync(request);

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_cuando_CreateAsync_falla_lanza_AuthException_y_revierte_transaccion()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "La contraseña es muy débil." }));

        var request = new RegisterRequest
        {
            Email = "debil@correo.com",
            Password = "123",
            NombreCompleto = "Carlos Ruiz",
            Rol = RolUsuario.Asesor
        };

        // Act
        var accion = async () => await _sut.RegistrarAsync(request);

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
        _unitOfWorkMock.Verify(u => u.RevertirTransaccionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- LoginAsync ----------

    [Fact]
    public async Task LoginAsync_con_credenciales_validas_devuelve_token()
    {
        // Arrange
        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "valido@correo.com",
            NombreCompleto = "Ana López",
            Rol = RolUsuario.Asesor
        };
        _userManagerMock.Setup(m => m.FindByEmailAsync(usuario.Email)).ReturnsAsync(usuario);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(usuario, It.IsAny<string>())).ReturnsAsync(true);
        _jwtGeneratorMock.Setup(j => j.GenerarToken(
                usuario.Id, usuario.Email!, usuario.Rol, usuario.NombreCompleto))
            .Returns(("token-valido", DateTime.UtcNow.AddHours(1)));

        var request = new LoginRequest { Email = usuario.Email, Password = "Clave123!" };

        // Act
        var resultado = await _sut.LoginAsync(request);

        // Assert
        resultado.Token.Should().Be("token-valido");
        resultado.UsuarioId.Should().Be(usuario.Id);
    }

    [Fact]
    public async Task LoginAsync_con_usuario_inexistente_lanza_AuthException()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var request = new LoginRequest { Email = "noexiste@correo.com", Password = "cualquiera" };

        // Act
        var accion = async () => await _sut.LoginAsync(request);

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
    }

    [Fact]
    public async Task LoginAsync_con_contrasena_incorrecta_lanza_AuthException()
    {
        // Arrange
        var usuario = new ApplicationUser { Id = Guid.NewGuid(), Email = "usuario@correo.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(usuario.Email)).ReturnsAsync(usuario);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(usuario, It.IsAny<string>())).ReturnsAsync(false);

        var request = new LoginRequest { Email = usuario.Email, Password = "incorrecta" };

        // Act
        var accion = async () => await _sut.LoginAsync(request);

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
    }

    // ---------- ObtenerPerfilAsync ----------

    [Fact]
    public async Task ObtenerPerfilAsync_devuelve_datos_del_usuario()
    {
        // Arrange
        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "perfil@correo.com",
            NombreCompleto = "Laura Díaz",
            Rol = RolUsuario.Estudiante,
            FechaRegistro = DateTime.UtcNow.AddMonths(-2)
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);

        // Act
        var resultado = await _sut.ObtenerPerfilAsync(usuario.Id);

        // Assert
        resultado.NombreCompleto.Should().Be("Laura Díaz");
        resultado.Email.Should().Be("perfil@correo.com");
    }

    [Fact]
    public async Task ObtenerPerfilAsync_con_usuario_inexistente_lanza_AuthException()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var accion = async () => await _sut.ObtenerPerfilAsync(Guid.NewGuid());

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
    }

    // ---------- ActualizarPerfilAsync ----------

    [Fact]
    public async Task ActualizarPerfilAsync_sin_cambiar_email_solo_actualiza_nombre()
    {
        // Arrange
        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "mismo@correo.com",
            NombreCompleto = "Nombre Viejo"
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(m => m.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);

        var request = new ActualizarPerfilRequest { NombreCompleto = "Nombre Nuevo", Email = "mismo@correo.com" };

        // Act
        var resultado = await _sut.ActualizarPerfilAsync(usuario.Id, request);

        // Assert
        resultado.NombreCompleto.Should().Be("Nombre Nuevo");
        _userManagerMock.Verify(m => m.SetEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_con_email_ya_usado_por_otro_lanza_AuthException()
    {
        // Arrange
        var usuario = new ApplicationUser { Id = Guid.NewGuid(), Email = "propio@correo.com", NombreCompleto = "Yo" };
        var otroUsuario = new ApplicationUser { Id = Guid.NewGuid(), Email = "ocupado@correo.com" };

        _userManagerMock.Setup(m => m.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(m => m.FindByEmailAsync("ocupado@correo.com")).ReturnsAsync(otroUsuario);

        var request = new ActualizarPerfilRequest { NombreCompleto = "Yo", Email = "ocupado@correo.com" };

        // Act
        var accion = async () => await _sut.ActualizarPerfilAsync(usuario.Id, request);

        // Assert
        await accion.Should().ThrowAsync<AuthException>();
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}