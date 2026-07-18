namespace EduApoyos.Domain.Entities;

/// <summary>
/// Datos académicos de un estudiante.
/// </summary>
public class Estudiante
{
    public Guid Id { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public string NumeroDocumento { get; private set; } = default!;
    public string TipoDocumento { get; private set; } = default!;
    public string ProgramaAcademico { get; private set; } = default!;
    public int Semestre { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime? FechaEliminacion { get; private set; }

    private readonly List<SolicitudApoyo> _solicitudes = new();
    public IReadOnlyCollection<SolicitudApoyo> Solicitudes => _solicitudes.AsReadOnly();

    protected Estudiante()
    {
    }

    public Estudiante(string numeroDocumento, string tipoDocumento,
        string programaAcademico, int semestre, Guid? usuarioId = null)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new Domain.Common.DomainException("El número de documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(tipoDocumento))
            throw new Domain.Common.DomainException("El tipo de documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(programaAcademico))
            throw new Domain.Common.DomainException("El programa académico es obligatorio.");
        if (semestre <= 0)
            throw new Domain.Common.DomainException("El semestre debe ser mayor a cero.");
        if (usuarioId.HasValue && usuarioId.Value == Guid.Empty)
            throw new Domain.Common.DomainException("El usuarioId, si se especifica, debe ser válido.");

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        NumeroDocumento = numeroDocumento;
        TipoDocumento = tipoDocumento;
        ProgramaAcademico = programaAcademico;
        Semestre = semestre;
        Activo = true;
    }

    public void ActualizarDatos(string programaAcademico, int semestre)
    {
        AsegurarActivo();

        if (string.IsNullOrWhiteSpace(programaAcademico))
            throw new Domain.Common.DomainException("El programa académico es obligatorio.");
        if (semestre <= 0)
            throw new Domain.Common.DomainException("El semestre debe ser mayor a cero.");

        ProgramaAcademico = programaAcademico;
        Semestre = semestre;
    }

    public void ActualizarDatosAcademicos(
        string tipoDocumento, string numeroDocumento, string programaAcademico, int semestre)
    {
        AsegurarActivo();

        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new Domain.Common.DomainException("El número de documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(tipoDocumento))
            throw new Domain.Common.DomainException("El tipo de documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(programaAcademico))
            throw new Domain.Common.DomainException("El programa académico es obligatorio.");
        if (semestre <= 0)
            throw new Domain.Common.DomainException("El semestre debe ser mayor a cero.");

        TipoDocumento = tipoDocumento;
        NumeroDocumento = numeroDocumento;
        ProgramaAcademico = programaAcademico;
        Semestre = semestre;
    }

    // Vincula la cuenta de usuario cuando el estudiante se registra en el portal.
    // Solo se puede hacer una vez.
    public void VincularUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new Domain.Common.DomainException("El usuarioId debe ser válido.");
        if (UsuarioId.HasValue)
            throw new Domain.Common.DomainException("El estudiante ya tiene una cuenta de usuario vinculada.");

        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Desactiva al estudiante (eliminación lógica). No verifica solicitudes en curso:
    /// esa regla depende de datos externos a la entidad y se valida en la capa Application
    /// (Command Handler) antes de invocar este método.
    /// </summary>
    public void Desactivar()
    {
        if (!Activo)
            throw new Domain.Common.DomainException("El estudiante ya se encuentra inactivo.");

        Activo = false;
        FechaEliminacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactiva a un estudiante previamente desactivado.
    /// </summary>
    public void Reactivar()
    {
        if (Activo)
            throw new Domain.Common.DomainException("El estudiante ya se encuentra activo.");

        Activo = true;
        FechaEliminacion = null;
    }

    private void AsegurarActivo()
    {
        if (!Activo)
            throw new Domain.Common.DomainException("No se puede modificar un estudiante inactivo.");
    }

    public bool TieneCuentaActiva => UsuarioId.HasValue;
}