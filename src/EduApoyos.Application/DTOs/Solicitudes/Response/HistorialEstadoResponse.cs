using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Solicitudes.Response
{
    public class HistorialEstadoResponse
    {
        public EstadoSolicitud EstadoAnterior { get; set; }

        public EstadoSolicitud EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; }

        public Guid UsuarioId { get; set; }

        public string? Observacion { get; set; }
    }
}
