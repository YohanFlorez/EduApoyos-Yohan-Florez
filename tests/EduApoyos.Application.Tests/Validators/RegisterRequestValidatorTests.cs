
    using EduApoyos.Application.DTOs.Auth;
    using EduApoyos.Application.DTOs.Auth.Request;
    using EduApoyos.Application.Validators;
    using EduApoyos.Domain.Enums;

    using Xunit;

    namespace EduApoyos.Application.Tests.Validators;

    public class RegisterRequestValidatorTests
    {
        private readonly RegisterRequestValidator _validator = new();

        private static RegisterRequest RequestValidoAsesor() => new()
        {
            NombreCompleto = "Juan Pérez",
            Email = "juan.perez@correo.com",
            Password = "Clave123",
            Rol = RolUsuario.Asesor
        };

        private static RegisterRequest RequestValidoEstudiante() => new()
        {
            NombreCompleto = "María Gómez",
            Email = "maria.gomez@correo.com",
            Password = "Clave123",
            Rol = RolUsuario.Estudiante,
            NumeroDocumento = "100200300",
            TipoDocumento = "CC",
            ProgramaAcademico = "Ingeniería de Sistemas",
            Semestre = 3
        };

        [Fact]
        public void Request_Valido_Asesor_NoTieneErrores()
        {
            var resultado = _validator.Validate(RequestValidoAsesor());

            Assert.True(resultado.IsValid);
        }

        [Fact]
        public void Request_Valido_Estudiante_NoTieneErrores()
        {
            var resultado = _validator.Validate(RequestValidoEstudiante());

            Assert.True(resultado.IsValid);
        }

        [Fact]
        public void Request_SinNombreCompleto_TieneError()
        {
            var request = RequestValidoAsesor();
            request.NombreCompleto = "";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.NombreCompleto));
        }

        [Fact]
        public void Request_NombreCompletoExcedeLongitudMaxima_TieneError()
        {
            var request = RequestValidoAsesor();
            request.NombreCompleto = new string('a', 151);

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.NombreCompleto));
        }

        [Theory]
        [InlineData("")]
        [InlineData("correo-invalido")]
        public void Request_EmailInvalidoOVacio_TieneError(string email)
        {
            var request = RequestValidoAsesor();
            request.Email = email;

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Email));
        }

        [Fact]
        public void Request_PasswordVacio_TieneError()
        {
            var request = RequestValidoAsesor();
            request.Password = "";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
        }

        [Fact]
        public void Request_PasswordMenorA8Caracteres_TieneError()
        {
            var request = RequestValidoAsesor();
            request.Password = "Abc123";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
        }

        [Fact]
        public void Request_PasswordSinMayuscula_TieneError()
        {
            var request = RequestValidoAsesor();
            request.Password = "clave123";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e =>
                e.PropertyName == nameof(RegisterRequest.Password) &&
                e.ErrorMessage.Contains("mayúscula"));
        }

        [Fact]
        public void Request_PasswordSinNumero_TieneError()
        {
            var request = RequestValidoAsesor();
            request.Password = "ClaveSegura";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e =>
                e.PropertyName == nameof(RegisterRequest.Password) &&
                e.ErrorMessage.Contains("número"));
        }

        [Fact]
        public void Request_RolFueraDeEnum_TieneError()
        {
            var request = RequestValidoAsesor();
            request.Rol = (RolUsuario)999;

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Rol));
        }

        [Fact]
        public void Request_Estudiante_SinNumeroDocumento_TieneError()
        {
            var request = RequestValidoEstudiante();
            request.NumeroDocumento = "";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.NumeroDocumento));
        }

        [Fact]
        public void Request_Estudiante_SinTipoDocumento_TieneError()
        {
            var request = RequestValidoEstudiante();
            request.TipoDocumento = "";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.TipoDocumento));
        }

        [Fact]
        public void Request_Estudiante_SinProgramaAcademico_TieneError()
        {
            var request = RequestValidoEstudiante();
            request.ProgramaAcademico = "";

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.ProgramaAcademico));
        }

        [Fact]
        public void Request_Estudiante_SemestreNulo_TieneError()
        {
            var request = RequestValidoEstudiante();
            request.Semestre = null;

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Semestre));
        }

        [Fact]
        public void Request_Estudiante_SemestreCeroOMenor_TieneError()
        {
            var request = RequestValidoEstudiante();
            request.Semestre = 0;

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(RegisterRequest.Semestre));
        }

        [Fact]
        public void Request_Asesor_NoRequiereCamposDeEstudiante()
        {
            var request = RequestValidoAsesor();
            // Campos de estudiante quedan vacíos/nulos, pero no deben generar error porque Rol != Estudiante
            request.NumeroDocumento = null;
            request.TipoDocumento = null;
            request.ProgramaAcademico = null;
            request.Semestre = null;

            var resultado = _validator.Validate(request);

            Assert.True(resultado.IsValid);
        }
    }
