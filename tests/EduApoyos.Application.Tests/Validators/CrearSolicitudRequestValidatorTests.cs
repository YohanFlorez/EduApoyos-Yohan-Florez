using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.Validators;
using EduApoyos.Domain.Enums;
using Xunit;

namespace EduApoyos.Application.Tests.Validators;

public class CrearSolicitudRequestValidatorTests
{
    private readonly CrearSolicitudRequestValidator _validator = new();

    private static CrearSolicitudRequest RequestValido() => new()
    {
        EstudianteId = Guid.NewGuid(),
        TipoApoyo = TipoApoyo.Beca,
        MontoSolicitado = 1_000_000,
        Descripcion = "Solicitud de apoyo económico para el semestre."
    };

    [Fact]
    public void Request_Valido_NoTieneErrores()
    {
        var resultado = _validator.Validate(RequestValido());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Request_EstudianteIdVacio_TieneError()
    {
        var request = RequestValido();
        request.EstudianteId = Guid.Empty;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearSolicitudRequest.EstudianteId));
    }

    [Fact]
    public void Request_TipoApoyoFueraDeEnum_TieneError()
    {
        var request = RequestValido();
        request.TipoApoyo = (TipoApoyo)999;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearSolicitudRequest.TipoApoyo));
    }

    [Fact]
    public void Request_MontoSolicitadoCeroOMenor_TieneError()
    {
        var request = RequestValido();
        request.MontoSolicitado = 0;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e =>
            e.PropertyName == nameof(CrearSolicitudRequest.MontoSolicitado) &&
            e.ErrorMessage.Contains("mayor a cero"));
    }

    [Fact]
    public void Request_MontoSolicitadoSuperaElMaximo_TieneError()
    {
        var request = RequestValido();
        request.MontoSolicitado = 50_000_001;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e =>
            e.PropertyName == nameof(CrearSolicitudRequest.MontoSolicitado) &&
            e.ErrorMessage.Contains("máximo permitido"));
    }

    [Fact]
    public void Request_MontoSolicitadoEnElLimiteMaximo_NoTieneError()
    {
        var request = RequestValido();
        request.MontoSolicitado = 50_000_000;

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Request_DescripcionVacia_TieneError()
    {
        var request = RequestValido();
        request.Descripcion = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearSolicitudRequest.Descripcion));
    }

    [Fact]
    public void Request_DescripcionExcedeLongitudMaxima_TieneError()
    {
        var request = RequestValido();
        request.Descripcion = new string('a', 1001);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearSolicitudRequest.Descripcion));
    }
}

