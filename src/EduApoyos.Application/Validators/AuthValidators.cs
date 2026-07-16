using EduApoyos.Application.DTOs.Auth;
using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");
        RuleFor(x => x.Rol).IsInEnum();

        When(x => x.Rol == RolUsuario.Estudiante, () =>
        {
            RuleFor(x => x.NumeroDocumento).NotEmpty().WithMessage("El número de documento es obligatorio para estudiantes.");
            RuleFor(x => x.TipoDocumento).NotEmpty().WithMessage("El tipo de documento es obligatorio para estudiantes.");
            RuleFor(x => x.ProgramaAcademico).NotEmpty().WithMessage("El programa académico es obligatorio para estudiantes.");
            RuleFor(x => x.Semestre).NotNull().GreaterThan(0).WithMessage("El semestre debe ser mayor a cero.");
        });
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
