using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Estudiantes.Request
{
    public sealed record ActualizarEstudianteRequest(
     string TipoDocumento,
     string NumeroDocumento,
     string ProgramaAcademico,
     int Semestre);
}
