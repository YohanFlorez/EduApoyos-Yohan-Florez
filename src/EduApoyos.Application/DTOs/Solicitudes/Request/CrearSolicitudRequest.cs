using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Solicitudes.Request
{
    public class CrearSolicitudRequest
    {
        public Guid EstudianteId { get; set; }

        public TipoApoyo TipoApoyo { get; set; }

        public decimal MontoSolicitado { get; set; }

        public string Descripcion { get; set; } = string.Empty;
    }
}
