using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.Validators;
using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EduApoyos.Application.Tests.Validators
{
    public class CambiarEstadoRequestValidatorTests
    {
        private readonly CambiarEstadoRequestValidator _validator = new();

        private static CambiarEstadoRequest RequestValido() => new()
        {
            NuevoEstado = EstadoSolicitud.Aprobada,
            Observacion = "Cumple con todos los requisitos."
        };

        [Fact]
        public void Request_Valido_NoTieneErrores()
        {
            var resultado = _validator.Validate(RequestValido());

            Assert.True(resultado.IsValid);
        }

        [Fact]
        public void Request_NuevoEstadoFueraDeEnum_TieneError()
        {
            var request = RequestValido();
            request.NuevoEstado = (EstadoSolicitud)999;

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CambiarEstadoRequest.NuevoEstado));
        }

        [Fact]
        public void Request_ObservacionExcedeLongitudMaxima_TieneError()
        {
            var request = RequestValido();
            request.Observacion = new string('a', 501);

            var resultado = _validator.Validate(request);

            Assert.False(resultado.IsValid);
            Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CambiarEstadoRequest.Observacion));
        }

        [Fact]
        public void Request_ObservacionVacia_NoTieneError()
        {
            // Observacion no tiene NotEmpty(), solo MaximumLength, así que vacío/nulo es válido.
            var request = RequestValido();
            request.Observacion = "";

            var resultado = _validator.Validate(request);

            Assert.True(resultado.IsValid);
        }
    }
}
