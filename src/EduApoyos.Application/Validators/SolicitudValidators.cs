using EduApoyos.Application.DTOs.Solicitudes;
using EduApoyos.Application.DTOs.Solicitudes.Request;
using FluentValidation;

namespace EduApoyos.Application.Validators;

public class CrearSolicitudRequestValidator : AbstractValidator<CrearSolicitudRequest>
{
    public CrearSolicitudRequestValidator()
    {
        RuleFor(x => x.EstudianteId).NotEmpty();
        RuleFor(x => x.TipoApoyo).IsInEnum();
        RuleFor(x => x.MontoSolicitado).GreaterThan(0)
            .WithMessage("El monto solicitado debe ser mayor a cero.")
            .LessThanOrEqualTo(50_000_000)
            .WithMessage("El monto solicitado supera el máximo permitido.");
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(1000);
    }
}

public class CambiarEstadoRequestValidator : AbstractValidator<CambiarEstadoRequest>
{
    public CambiarEstadoRequestValidator()
    {
        RuleFor(x => x.NuevoEstado).IsInEnum();
        RuleFor(x => x.Observacion).MaximumLength(500);
    }
}
