using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Persistence;

namespace EduApoyos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EduApoyosDbContext _context;

    public IEstudianteRepository Estudiantes { get; }
    public ISolicitudRepository Solicitudes { get; }

    public UnitOfWork(EduApoyosDbContext context, IEstudianteRepository estudiantes, ISolicitudRepository solicitudes)
    {
        _context = context;
        Estudiantes = estudiantes;
        Solicitudes = solicitudes;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
