namespace EduApoyos.Domain.Interfaces;

/// agrupa los repositorios y confirma los cambios en una sola transacción
public interface IUnitOfWork
{
    IEstudianteRepository Estudiantes { get; }
    ISolicitudRepository Solicitudes { get; }
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);

    
    Task IniciarTransaccionAsync(CancellationToken ct = default);

    /// <summary>Confirma la transacción iniciada con IniciarTransaccionAsync.</summary>
    Task ConfirmarTransaccionAsync(CancellationToken ct = default);

    /// <summary>Revierte la transacción iniciada con IniciarTransaccionAsync.</summary>
    Task RevertirTransaccionAsync(CancellationToken ct = default);
}