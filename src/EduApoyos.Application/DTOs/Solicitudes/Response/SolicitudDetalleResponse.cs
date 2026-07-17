using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Solicitudes.Response
{
    public class SolicitudDetalleResponse
    {
        public Guid Id { get; set; }

        public Guid EstudianteId { get; set; }

        public TipoApoyo TipoApoyo { get; set; }

        public decimal MontoSolicitado { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public EstadoSolicitud Estado { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public Guid? AsesorId { get; set; }

        public IReadOnlyList<HistorialEstadoResponse> Historial { get; set; }
            = new List<HistorialEstadoResponse>();
    }
}
