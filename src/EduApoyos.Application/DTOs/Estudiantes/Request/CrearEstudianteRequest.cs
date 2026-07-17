namespace EduApoyos.Application.DTOs.Estudiantes.Request
{
    public class CrearEstudianteRequest
    {
        public Guid? UsuarioId { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = string.Empty;

        public string ProgramaAcademico { get; set; } = string.Empty;

        public int Semestre { get; set; }
    }
}