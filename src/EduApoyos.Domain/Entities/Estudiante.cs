namespace EduApoyos.Domain.Entities;

/// 
/// Datos académicos de un estudiante. 
///
public class Estudiante
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string NumeroDocumento { get; private set; } = default!;
    public string TipoDocumento { get; private set; } = default!;
    public string ProgramaAcademico { get; private set; } = default!;
    public int Semestre { get; private set; }

    private readonly List<SolicitudApoyo> _solicitudes = new();
    public IReadOnlyCollection<SolicitudApoyo> Solicitudes => _solicitudes.AsReadOnly();

    protected Estudiante()
    {
       
    }

    public Estudiante(Guid usuarioId, string numeroDocumento, string tipoDocumento,
        string programaAcademico, int semestre)
    {
        if (usuarioId == Guid.Empty)
            throw new Domain.Common.DomainException("El estudiante debe estar asociado a un usuario válido.");
        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new Domain.Common.DomainException("El número de documento es obligatorio.");
        if (semestre <= 0)
            throw new Domain.Common.DomainException("El semestre debe ser mayor a cero.");

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        NumeroDocumento = numeroDocumento;
        TipoDocumento = tipoDocumento;
        ProgramaAcademico = programaAcademico;
        Semestre = semestre;
    }

    public void ActualizarDatos(string programaAcademico, int semestre)
    {
        if (semestre <= 0)
            throw new Domain.Common.DomainException("El semestre debe ser mayor a cero.");

        ProgramaAcademico = programaAcademico;
        Semestre = semestre;
    }
}
