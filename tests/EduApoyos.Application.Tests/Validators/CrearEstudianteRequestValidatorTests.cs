using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.Validators;
using Xunit;

namespace EduApoyos.Application.Tests.Validators;

public class CrearEstudianteRequestValidatorTests
{
    private readonly CrearEstudianteRequestValidator _validator = new();


   

    private static CrearEstudianteRequest RequestValido() => new()
    {
        UsuarioId = Guid.NewGuid(),
        NumeroDocumento = "100200300",
        TipoDocumento = "CC",
        ProgramaAcademico = "Ingeniería",
        Semestre = 3
    };

    [Fact]
    public void Request_Valido_NoTieneErrores()
    {
        var resultado = _validator.Validate(RequestValido());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Request_UsuarioIdEnGuidEmpty_NoTieneError()
    {
        var request = RequestValido();
        request.UsuarioId = Guid.Empty;

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }

    


    [Fact]
    public void Request_SinNumeroDocumento_TieneError()
    {
        var request = RequestValido();
        request.NumeroDocumento = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.NumeroDocumento));
    }

    [Fact]
    public void Request_NumeroDocumentoExcedeLongitudMaxima_TieneError()
    {
        var request = RequestValido();
        request.NumeroDocumento = new string('1', 31);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.NumeroDocumento));
    }

    [Fact]
    public void Request_SinTipoDocumento_TieneError()
    {
        var request = RequestValido();
        request.TipoDocumento = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.TipoDocumento));
    }

    [Fact]
    public void Request_TipoDocumentoExcedeLongitudMaxima_TieneError()
    {
        var request = RequestValido();
        request.TipoDocumento = new string('A', 21);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.TipoDocumento));
    }

    [Fact]
    public void Request_SinProgramaAcademico_TieneError()
    {
        var request = RequestValido();
        request.ProgramaAcademico = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.ProgramaAcademico));
    }

    [Fact]
    public void Request_ProgramaAcademicoExcedeLongitudMaxima_TieneError()
    {
        var request = RequestValido();
        request.ProgramaAcademico = new string('a', 151);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.ProgramaAcademico));
    }

    [Fact]
    public void Request_SemestreCeroOMenor_TieneError()
    {
        var request = RequestValido();
        request.Semestre = 0;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.Semestre));
    }

    [Fact]
    public void Request_SemestreSuperaElMaximo_TieneError()
    {
        var request = RequestValido();
        request.Semestre = 21;

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CrearEstudianteRequest.Semestre));
    }

    [Fact]
    public void Request_SemestreEnElLimiteMaximo_NoTieneError()
    {
        var request = RequestValido();
        request.Semestre = 20;

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }
}