using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Solicitudes.Request
{
    public class CambiarEstadoRequest
    {
        public EstadoSolicitud NuevoEstado { get; set; }

        public string? Observacion { get; set; }
    }
}
