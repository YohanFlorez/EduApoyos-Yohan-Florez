using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduApoyos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EduApoyosDbContext _context;
    private IDbContextTransaction? _transaccionActual;

    public IEstudianteRepository Estudiantes { get; }
    public ISolicitudRepository Solicitudes { get; }

    public UnitOfWork(EduApoyosDbContext context, IEstudianteRepository estudiantes, ISolicitudRepository solicitudes)
    {
        _context = context;
        Estudiantes = estudiantes;
        Solicitudes = solicitudes;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public async Task IniciarTransaccionAsync(CancellationToken ct = default)
    {
        // Si ya hay una transacción activa (ej. por un handler que también la abre),
        // no abrimos otra anidada; EF Core no soporta transacciones anidadas nativamente.
        if (_transaccionActual is not null) return;

        _transaccionActual = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task ConfirmarTransaccionAsync(CancellationToken ct = default)
    {
        if (_transaccionActual is null) return;

        try
        {
            await _transaccionActual.CommitAsync(ct);
        }
        finally
        {
            await _transaccionActual.DisposeAsync();
            _transaccionActual = null;
        }
    }

    public async Task RevertirTransaccionAsync(CancellationToken ct = default)
    {
        if (_transaccionActual is null) return;

        try
        {
            await _transaccionActual.RollbackAsync(ct);
        }
        finally
        {
            await _transaccionActual.DisposeAsync();
            _transaccionActual = null;
        }
    }
}