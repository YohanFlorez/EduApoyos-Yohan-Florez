using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.Validators;
using Xunit;

namespace EduApoyos.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Request_Valido_NoTieneErrores()
    {
        var request = new LoginRequest
        {
            Email = "usuario@correo.com",
            Password = "Clave123"
        };

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Request_EmailVacio_TieneError()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = "Clave123"
        };

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Request_EmailFormatoInvalido_TieneError()
    {
        var request = new LoginRequest
        {
            Email = "no-es-un-correo",
            Password = "Clave123"
        };

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Request_PasswordVacio_TieneError()
    {
        var request = new LoginRequest
        {
            Email = "usuario@correo.com",
            Password = ""
        };

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(LoginRequest.Password));
    }
}