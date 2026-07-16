using EduApoyos.Application.DTOs.Estudiantes;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using FluentValidation;

namespace EduApoyos.Application.Validators;

public class CrearEstudianteRequestValidator : AbstractValidator<CrearEstudianteRequest>
{
    public CrearEstudianteRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.NumeroDocumento).NotEmpty().MaximumLength(30);
        RuleFor(x => x.TipoDocumento).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ProgramaAcademico).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Semestre).GreaterThan(0).LessThanOrEqualTo(20);
    }
}
