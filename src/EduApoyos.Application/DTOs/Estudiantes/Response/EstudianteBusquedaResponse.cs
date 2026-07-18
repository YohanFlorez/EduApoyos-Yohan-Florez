using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Estudiantes.Response
{
    public class EstudianteBusquedaResponse
    {
        public Guid Id { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;

        public string NumeroDocumento { get; set; } = string.Empty;

        public string ProgramaAcademico { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; }
    }
}
